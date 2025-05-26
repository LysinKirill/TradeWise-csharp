using System;

namespace TradeWiseBackend.Bll.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Model;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.RepositoryModels;
using System.Linq;
using User;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Invest;

public class StrategyStatusUpdateWorker(
    IServiceProvider serviceProvider,
    ILogger<StrategyStatusUpdateWorker> logger
    ) : BackgroundService
{

    private StageExecutionStatus MapExecutionStatus(ExecutionStatus status)
    {
        return status switch
        {
            ExecutionStatus.Completed => StageExecutionStatus.Completed,
            ExecutionStatus.Failed => StageExecutionStatus.Failed,
            ExecutionStatus.Running => StageExecutionStatus.Running,
            ExecutionStatus.Pending => StageExecutionStatus.Pending,
            _ => throw new NotImplementedException(),
        };
    }

    private Invest.StatType MapStatType(Domain.Models.StatType stat)
    {
        return stat switch
        {
            Domain.Models.StatType.BollingerBandLower => Invest.StatType.BollingerBandLower,
            Domain.Models.StatType.BollingerBandMiddle => Invest.StatType.BollingerBandMiddle,
            Domain.Models.StatType.BollingerBandUpper => Invest.StatType.BollingerBandUpper,
            Domain.Models.StatType.ExponentialMovingAverage => Invest.StatType.ExponentialMovingAverage,
            Domain.Models.StatType.MovingAverage => Invest.StatType.MovingAverage,
            Domain.Models.StatType.MovingAverageConvergenceDivergence => Invest.StatType.MovingAverageConvergenceDivergence,
            Domain.Models.StatType.RelativeStrengthIndex => Invest.StatType.RelativeStrengthIndex,
            _ => throw new InvalidCastException($"Unknown StatType {stat}")
        };
    }

    private async Task<bool> TransitionConditionPassed(StageInfo info, IStrategyRepository strategyRepository, InvestService.InvestServiceClient investServiceClient, ITokenService tokenService, IAccountRepository accountRepository, CancellationToken ct)
    {
        var transitions = await strategyRepository.FetchTransitionByDestinationStage(info.Id);
        bool checkPassed = true;
        foreach (var transition in transitions)
        {
            var request = new GetInstrumentStatRequest
            {
                InstrumentId = transition.InstrumentId,
                StatType = MapStatType(transition.StatType),
                From = Timestamp.FromDateTime(DateTime.Now.AddMinutes(-600).ToUniversalTime()),
                To = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };

            var user = await accountRepository.GetUserById(info.UserId);
            var token = await tokenService.GenerateToken(new AccountEntityModel
            {
                Id = info.UserId,
                Email = user!.Email
            });
            var meta = new Metadata { { "Authorization", $"Bearer {token}" } };
            var instrumentStat =
                await investServiceClient.GetInstrumentStatAsync(request, headers: meta, cancellationToken: ct);
            switch (transition.Operation)
            {
                case TransitionConditionType.EqualTo:
                    checkPassed = instrumentStat.StatValue == transition.Value;
                    break;
                case TransitionConditionType.GreaterThan:
                    checkPassed = instrumentStat.StatValue > transition.Value;
                    break;
                case TransitionConditionType.LessThan:
                    checkPassed = instrumentStat.StatValue < transition.Value;
                    break;
            }
            if (!checkPassed) return false;
        }
        return true;
    }

    private async Task<List<StageInfo>> GetExecutableNodes(IStrategyRepository strategyRepository, InvestService.InvestServiceClient investServiceClient, ITokenService tokenService, IAccountRepository accountRepository, CancellationToken ct)
    {
        var activeStrategyExecutions = await strategyRepository.GetPendingAndRunningStrategies();
        var executableNodes = new List<StageInfo>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var strategyStageExecutions = await strategyRepository.GetPendingStageExecutionsByStrategy(strategyExecution.StrategyId);

            foreach (var stageExecution in strategyStageExecutions)
            {
                var transitionsPrevStages = await strategyRepository.FetchTransitionByDestinationStage(stageExecution.StageId);
                if (transitionsPrevStages == null || transitionsPrevStages.Count == 0)
                {
                    var stageInfo = await strategyRepository.FetchStageWithUserByStageId(stageExecution.Id);
                    if (await TransitionConditionPassed(stageInfo, strategyRepository, investServiceClient, tokenService, accountRepository, ct))
                    {
                        executableNodes.Add(stageInfo);
                    }
                    else
                    {
                        await CancelStage(strategyRepository, stageInfo.Id, stageInfo.StrategyId, stageInfo.StrategyExecutionId, ct);
                    }
                    break;
                }

                var previousStageExecution = await strategyRepository.FetchStageExecutionByStageId(transitionsPrevStages[0].StageSourceId, strategyExecution.Id);
                if (previousStageExecution.Status == StageExecutionStatus.Completed || previousStageExecution.Status == StageExecutionStatus.Failed)
                {
                    var stageInfo = await strategyRepository.FetchStageWithUserByStageId(stageExecution.Id);
                    if (await TransitionConditionPassed(stageInfo, strategyRepository, investServiceClient, tokenService, accountRepository, ct))
                    {
                        executableNodes.Add(stageInfo);
                    }
                    else
                    {
                        await CancelStage(strategyRepository, stageInfo.Id, stageInfo.StrategyId, stageInfo.StrategyExecutionId, ct);
                    }
                }
            }
        }

        return executableNodes;
    }

    private async Task FailStage(IStrategyRepository strategyRepository, Guid stageId, Guid strategyId, Guid strategyExecutionId, CancellationToken ct)
    {
        var strategyTransitions = await strategyRepository.FetchTransitionByStrategyId(strategyId, ct);

        var transitionsLookup = strategyTransitions.ToLookup(t => t.StageSourceId, t => t.StageDestinationId);

        var descendants = new HashSet<Guid>();

        void TraverseDescendants(Guid currentStageId)
        {
            if (!transitionsLookup.Contains(currentStageId))
                return;

            foreach (var childStageId in transitionsLookup[currentStageId])
            {
                if (descendants.Add(childStageId))
                {
                    TraverseDescendants(childStageId);
                }
            }
        }

        TraverseDescendants(stageId);
        descendants.Add(stageId);

        await strategyRepository.FailStageExecutionsBulk(descendants.ToList(), strategyExecutionId, ct);
        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(strategyExecutionId, ct);
        if (activeExecutions.Count == 0)
        {
            await strategyRepository.UpdateStrategyExecutionStatus(strategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Failed, ct);
        }
    }

        private async Task CancelStage(IStrategyRepository strategyRepository, Guid stageId, Guid strategyId, Guid strategyExecutionId, CancellationToken ct)
    {
        var strategyTransitions = await strategyRepository.FetchTransitionByStrategyId(strategyId, ct);

        var transitionsLookup = strategyTransitions.ToLookup(t => t.StageSourceId, t => t.StageDestinationId);

        var descendants = new HashSet<Guid>();

        void TraverseDescendants(Guid currentStageId)
        {
            if (!transitionsLookup.Contains(currentStageId))
                return;

            foreach (var childStageId in transitionsLookup[currentStageId])
            {
                if (descendants.Add(childStageId))
                {
                    TraverseDescendants(childStageId);
                }
            }
        }

        TraverseDescendants(stageId);
        descendants.Add(stageId);

        await strategyRepository.CancelStageExecutionsBulk(descendants.ToList(), strategyExecutionId, ct);
        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(strategyExecutionId, ct);
        if (activeExecutions.Count == 0)
        {
            await strategyRepository.UpdateStrategyExecutionStatus(strategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Cancelled, ct);
        }
    }

    private async Task ProcessPendingNodes(IStrategyRepository strategyRepository,
        ModelService.ModelServiceClient modelServiceClient,
        UserService.UserServiceClient userServiceClient,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IAccountRepository accountRepository,
        InvestService.InvestServiceClient investServiceClient,
        CancellationToken ct)
    {
        logger.LogInformation("ProcessPendingNodes started.");

        var nextNodes = await GetExecutableNodes(strategyRepository, investServiceClient, tokenService, accountRepository, ct);
        var nodesGroupedByStrategies = nextNodes.GroupBy(s => s.StrategyId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var nodesGroup in nodesGroupedByStrategies)
        {
            var strategyId = nodesGroup.Key;
            var strategyNodes = nodesGroup.Value;
            int countNodes = strategyNodes.Count;
            var userId = strategyNodes[0].UserId;
            var allocatedBudget = strategyNodes[0].AllocatedBudget;
            var user = await accountRepository.GetUserById(userId);
            var token = await tokenService.GenerateToken(new AccountEntityModel
            {
                Id = userId,
                Email = user!.Email
            });

            var meta = new Metadata { { "Authorization", $"Bearer {token}" } };

            var potfolioInfo = await userServiceClient.GetPortfolioAsync(new Empty(), headers: meta, cancellationToken: ct);
            double balance = potfolioInfo.RubleBalance;
            var initialBalance = Math.Min(balance, allocatedBudget) / countNodes;
            logger.LogInformation($"Balance from python {potfolioInfo.RubleBalance}, initialBalance = {initialBalance}, count = {countNodes}");
            initialBalance = 1;

            foreach (var node in strategyNodes)
            {
                var request = new StartExecutionRequest
                {
                    ModelId = node.StageModel,
                    InitialBalance = initialBalance,
                    MaxExecutionDurationSeconds = node.MaxExecutionDurationSeconds,
                    IsPaperTrade = node.IsPaperTrade
                };

                var startExecutionResponse = new StartExecutionResponse();
                try
                {
                    startExecutionResponse = await modelServiceClient.StartExecutionAsync(request, headers: meta, cancellationToken: ct);
                }
                catch (RpcException ex)
                {
                    logger.LogError($"Failed to run stage {node.Id} execution {node.StageExecutionId}\n{ex.Message}");
                    await FailStage(strategyRepository, node.Id, node.StrategyId, node.StrategyExecutionId, ct);
                    continue;
                }

                await unitOfWork.BeginTransactionAsync();
                try
                {
                    await strategyRepository.SaveExternalExecutionId(node.StageExecutionId, startExecutionResponse.ExecutionId, ct);
                    await strategyRepository.UpdateStageExecutionStatus(node.StageExecutionId, StageExecutionStatus.Running, ct);
                    await strategyRepository.UpdateStrategyExecutionStatus(node.StrategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Running, ct);
                    await strategyRepository.BorrowMoneyFromAllocatedBudget(node.StrategyExecutionId, initialBalance, ct);
                    await unitOfWork.CommitAsync();
                }
                catch
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
        }
    }

    private async Task ProcessRunningNodes(IStrategyRepository strategyRepository, ModelService.ModelServiceClient modelServiceClient, IUnitOfWork unitOfWork, ITokenService tokenService, CancellationToken ct)
    {
        logger.LogInformation("ProcessRunningNodes started.");

        var runningStageExecutions = await strategyRepository.FetchRunningStageExecutionsWithUserInfo(ct);
        logger.LogInformation($"{runningStageExecutions.Count} running stages");
        foreach (var execution in runningStageExecutions)
        {
            if (!execution.ExternalExecutionId.HasValue)
            {
                throw new Exception($"Running stage {execution.StageId}, execution id {execution.Id} without ExternalExecutionId");
            }

            var request = new GetExecutionInfoRequest
            {
                ExecutionId = execution.ExternalExecutionId.Value
            };

            var token = await tokenService.GenerateToken(new AccountEntityModel
            {
                Id = execution.UserId,
                Email = execution.Email
            });

            var meta = new Metadata { { "Authorization", $"Bearer {token}" } };
            var response = await modelServiceClient.GetExecutionInfoAsync(request, headers: meta, cancellationToken: ct);

            if (MapExecutionStatus(response.Status) != execution.Status)
            {
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    if (response.Status == ExecutionStatus.Failed)
                    {
                        await FailStage(strategyRepository, execution.StageId, execution.StrategyId, execution.StrategyExecutionId, ct);
                    }
                    if (response.Status == ExecutionStatus.Completed)
                    {
                        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(execution.StrategyExecutionId, ct);
                        if (activeExecutions.Count == 1)
                        {
                            await strategyRepository.UpdateStrategyExecutionStatus(execution.StrategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Completed, ct);
                        }
                        await strategyRepository.RefundMoneyIntoAllocatedBudget(execution.StrategyExecutionId, response.MaxBudget, ct);
                        await strategyRepository.UpdateStageExecutionStatus(execution.Id, MapExecutionStatus(response.Status), ct);
                    }
                    await unitOfWork.CommitAsync();
                }
                catch
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // TODO: проверять переходы по статусам на корректность
        logger.LogInformation("StrategyExecutionScheduler started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var strategyRepository = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
                var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
                var userServiceClient = scope.ServiceProvider.GetRequiredService<UserService.UserServiceClient>();
                var modelServiceClient = scope.ServiceProvider.GetRequiredService<ModelService.ModelServiceClient>();
                var investServiceClient = scope.ServiceProvider.GetRequiredService<InvestService.InvestServiceClient>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

                await ProcessRunningNodes(strategyRepository, modelServiceClient, unitOfWork, tokenService, ct);
                await ProcessPendingNodes(strategyRepository, modelServiceClient, userServiceClient, unitOfWork, tokenService, accountRepository, investServiceClient, ct);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while scheduling strategy executions.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }

        logger.LogInformation("StrategyExecutionScheduler stopping.");
    }
}

