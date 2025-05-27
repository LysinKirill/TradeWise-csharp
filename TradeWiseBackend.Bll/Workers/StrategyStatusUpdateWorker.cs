using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Invest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Model;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.RepositoryModels;
using User;
using StatType = Invest.StatType;
using StrategyExecutionStatus = TradeWiseBackend.Domain.RepositoryModels.StrategyExecutionStatus;

namespace TradeWiseBackend.Bll.Services;

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
            _ => throw new InvalidCastException()
        };
    }

    private StatType MapStatType(Domain.Models.StatType stat)
    {
        return stat switch
        {
            Domain.Models.StatType.BollingerBandLower => StatType.BollingerBandLower,
            Domain.Models.StatType.BollingerBandMiddle => StatType.BollingerBandMiddle,
            Domain.Models.StatType.BollingerBandUpper => StatType.BollingerBandUpper,
            Domain.Models.StatType.ExponentialMovingAverage => StatType.ExponentialMovingAverage,
            Domain.Models.StatType.MovingAverage => StatType.MovingAverage,
            Domain.Models.StatType.MovingAverageConvergenceDivergence => StatType.MovingAverageConvergenceDivergence,
            Domain.Models.StatType.RelativeStrengthIndex => StatType.RelativeStrengthIndex,
            _ => throw new InvalidCastException($"Unknown StatType {stat}")
        };
    }

    private async Task<Metadata> GetMetaByUserId(IAccountRepository accountRepository, ITokenService tokenService,
        string userId)
    {
        var user = await accountRepository.GetUserById(userId);
        var token = await tokenService.GenerateToken(new AccountEntityModel
        {
            Id = userId,
            Email = user!.Email
        });
        var meta = new Metadata { { "Authorization", $"Bearer {token}" } };
        return meta;
    }

    private async Task<bool> TransitionConditionPassed(InfoForExecution info, IStrategyRepository strategyRepository,
        InvestService.InvestServiceClient investServiceClient, ITokenService tokenService,
        IAccountRepository accountRepository, CancellationToken ct)
    {
        var transitions = await strategyRepository.FetchTransitionByDestinationStage(info.Id, ct);
        var checkPassed = true;
        foreach (var transition in transitions)
        {
            var request = new GetInstrumentStatRequest
            {
                InstrumentId = transition.InstrumentId,
                StatType = MapStatType(transition.StatType),
                From = Timestamp.FromDateTime(DateTime.Now.AddMinutes(-600).ToUniversalTime()),
                To = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            var meta = await GetMetaByUserId(accountRepository, tokenService, info.UserId);
            var instrumentStat =
                await investServiceClient.GetInstrumentStatAsync(request, meta, cancellationToken: ct);

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

    private async Task<List<InfoForExecution>> GetExecutableNodes(IStrategyRepository strategyRepository,
        InvestService.InvestServiceClient investServiceClient, ITokenService tokenService,
        IAccountRepository accountRepository, CancellationToken ct)
    {
        var activeStrategyExecutions = await strategyRepository.FetchPendingAndRunningStrategies(ct);
        var executableNodes = new List<InfoForExecution>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var strategyStageExecutions =
                await strategyRepository.FetchPendingStageExecutionsByStrategyExecutionId(strategyExecution.Id, ct);

            foreach (var stageExecution in strategyStageExecutions)
            {
                var transitionsToPrevStage =
                    await strategyRepository.FetchTransitionByDestinationStage(stageExecution.StageId, ct);
                if (transitionsToPrevStage == null || transitionsToPrevStage.Count == 0)
                {
                    var stageInfo =
                        await strategyRepository.FetchStageWithUserIdByStageExecutionId(stageExecution.Id, ct);
                    if (await TransitionConditionPassed(stageInfo, strategyRepository, investServiceClient,
                            tokenService, accountRepository, ct))
                        executableNodes.Add(stageInfo);
                    else
                        await CancelStage(strategyRepository, stageInfo.Id, stageInfo.StrategyId,
                            stageInfo.StrategyExecutionId, ct);
                    break;
                }

                var previousStageExecution =
                    await strategyRepository.FetchStageExecutionByStageIdAndStrategyExecution(
                        transitionsToPrevStage[0].StageSourceId, strategyExecution.Id, ct);
                if (previousStageExecution.Status == StageExecutionStatus.Completed ||
                    previousStageExecution.Status == StageExecutionStatus.Failed)
                {
                    var stageInfo =
                        await strategyRepository.FetchStageWithUserIdByStageExecutionId(stageExecution.Id, ct);
                    if (await TransitionConditionPassed(stageInfo, strategyRepository, investServiceClient,
                            tokenService, accountRepository, ct))
                        executableNodes.Add(stageInfo);
                    else
                        await CancelStage(strategyRepository, stageInfo.Id, stageInfo.StrategyId,
                            stageInfo.StrategyExecutionId, ct);
                }
            }
        }

        return executableNodes;
    }

    private async Task FailStage(IStrategyRepository strategyRepository, Guid stageId, Guid strategyId,
        Guid strategyExecutionId, CancellationToken ct)
    {
        var strategyTransitions = await strategyRepository.FetchTransitionByStrategyId(strategyId, ct);

        var transitionsLookup = strategyTransitions.ToLookup(t => t.StageSourceId, t => t.StageDestinationId);

        var descendants = new HashSet<Guid>();

        void TraverseDescendants(Guid currentStageId)
        {
            if (!transitionsLookup.Contains(currentStageId))
                return;

            foreach (var childStageId in transitionsLookup[currentStageId])
                if (descendants.Add(childStageId))
                    TraverseDescendants(childStageId);
        }

        TraverseDescendants(stageId);
        descendants.Add(stageId);

        await strategyRepository.FailStageExecutionsBulk(descendants.ToList(), strategyExecutionId, ct);
        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(strategyExecutionId, ct);
        if (activeExecutions.Count == 0)
            await strategyRepository.UpdateStrategyExecutionStatusByStrategyExecution(strategyExecutionId,
                StrategyExecutionStatus.Failed, ct);
    }

    private async Task CancelStage(IStrategyRepository strategyRepository, Guid stageId, Guid strategyId,
        Guid strategyExecutionId, CancellationToken ct)
    {
        var strategyTransitions = await strategyRepository.FetchTransitionByStrategyId(strategyId, ct);

        var transitionsLookup = strategyTransitions.ToLookup(t => t.StageSourceId, t => t.StageDestinationId);

        var descendants = new HashSet<Guid>();

        void TraverseDescendants(Guid currentStageId)
        {
            if (!transitionsLookup.Contains(currentStageId))
                return;

            foreach (var childStageId in transitionsLookup[currentStageId])
                if (descendants.Add(childStageId))
                    TraverseDescendants(childStageId);
        }

        TraverseDescendants(stageId);
        descendants.Add(stageId);

        await strategyRepository.CancelStageExecutionsBulk(descendants.ToList(), strategyExecutionId, ct);
        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(strategyExecutionId, ct);
        if (activeExecutions.Count == 0)
            await strategyRepository.UpdateStrategyExecutionStatusByStrategyExecution(strategyExecutionId,
                StrategyExecutionStatus.Cancelled, ct);
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

        var nextNodes =
            await GetExecutableNodes(strategyRepository, investServiceClient, tokenService, accountRepository, ct);
        var nodesGroupedByStrategies = nextNodes.GroupBy(s => s.StrategyId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var nodesGroup in nodesGroupedByStrategies)
        {
            var strategyNodes = nodesGroup.Value;
            var countNodes = strategyNodes.Count;

            var strategyId = nodesGroup.Key;
            var userId = strategyNodes[0].UserId;

            var allocatedBudget = strategyNodes[0].AllocatedBudget;

            var meta = await GetMetaByUserId(accountRepository, tokenService, userId);
            var potfolioInfo = await userServiceClient.GetPortfolioAsync(new Empty(), meta, cancellationToken: ct);

            var balance = potfolioInfo.RubleBalance;
            var initialBalance = Math.Min(balance, allocatedBudget) / countNodes;
            logger.LogInformation(
                $"Balance from python {potfolioInfo.RubleBalance}, initialBalance = {initialBalance}, count = {countNodes}");

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
                    startExecutionResponse =
                        await modelServiceClient.StartExecutionAsync(request, meta, cancellationToken: ct);
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
                    await strategyRepository.SaveExternalExecutionId(node.StageExecutionId,
                        startExecutionResponse.ExecutionId, ct);
                    await strategyRepository.UpdateStageExecutionStatusByStageExecutionId(node.StageExecutionId,
                        StageExecutionStatus.Running, ct);
                    await strategyRepository.UpdateStrategyExecutionStatusByStrategyExecution(node.StrategyExecutionId,
                        StrategyExecutionStatus.Running, ct);
                    await strategyRepository.BorrowMoneyFromAllocatedBudget(node.StrategyExecutionId, initialBalance,
                        ct);
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

    private async Task ProcessRunningNodes(IStrategyRepository strategyRepository,
        ModelService.ModelServiceClient modelServiceClient, IAccountRepository accountRepository,
        IUnitOfWork unitOfWork, ITokenService tokenService, CancellationToken ct)
    {
        logger.LogInformation("ProcessRunningNodes started.");
        var runningStageExecutions = await strategyRepository.FetchRunningStageExecutionsWithUserInfo(ct);
        logger.LogInformation($"{runningStageExecutions.Count} running stages");

        foreach (var execution in runningStageExecutions)
        {
            if (!execution.ExternalExecutionId.HasValue)
                throw new Exception(
                    $"Running stage {execution.StageId}, execution id {execution.Id} without ExternalExecutionId");

            var request = new GetExecutionInfoRequest
            {
                ExecutionId = execution.ExternalExecutionId.Value
            };
            var meta = await GetMetaByUserId(accountRepository, tokenService, execution.UserId);
            var response = await modelServiceClient.GetExecutionInfoAsync(request, meta, cancellationToken: ct);

            if (MapExecutionStatus(response.Status) != execution.Status)
            {
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    if (response.Status == ExecutionStatus.Failed)
                        await FailStage(strategyRepository, execution.StageId, execution.StrategyId,
                            execution.StrategyExecutionId, ct);
                    if (response.Status == ExecutionStatus.Completed)
                    {
                        var activeExecutions =
                            await strategyRepository.FetchActiveStageExecutions(execution.StrategyExecutionId, ct);
                        if (activeExecutions.Count == 1)
                            await strategyRepository.UpdateStrategyExecutionStatusByStrategyExecution(
                                execution.StrategyExecutionId, StrategyExecutionStatus.Completed, ct);
                        await strategyRepository.RefundMoneyIntoAllocatedBudget(execution.StrategyExecutionId,
                            response.MaxBudget, ct);
                        await strategyRepository.UpdateStageExecutionStatusByStageExecutionId(execution.Id,
                            MapExecutionStatus(response.Status), ct);
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
        logger.LogInformation("StrategyExecutionScheduler started.");

        while (!ct.IsCancellationRequested)
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

                await ProcessRunningNodes(strategyRepository, modelServiceClient, accountRepository, unitOfWork,
                    tokenService, ct);
                await ProcessPendingNodes(strategyRepository, modelServiceClient, userServiceClient, unitOfWork,
                    tokenService, accountRepository, investServiceClient, ct);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while scheduling strategy executions.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }

        logger.LogInformation("StrategyExecutionScheduler stopping.");
    }
}