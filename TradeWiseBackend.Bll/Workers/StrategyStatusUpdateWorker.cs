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

    private async Task<List<StageInfo>> GetExecutableNodes(IStrategyRepository strategyRepository, CancellationToken ct)
    {
        var activeStrategyExecutions = await strategyRepository.GetPendingAndRunningStrategies();
        logger.LogInformation($"Active strategy executions started StrategyId->StrategyExecutionId:\n{string.Join(",", activeStrategyExecutions.Select(info => $"({info.StrategyId}->{info.Id}), "))}.\n\n");
        var executableNodes = new List<StageInfo>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var strategyStageExecutions = await strategyRepository.GetPendingStageExecutionsByStrategy(strategyExecution.StrategyId);
            logger.LogInformation($"Active stage executions started StageExecutionId->StageId->Status:\n{string.Join(", ", strategyStageExecutions.Select(info => $"({info.Id}->{info.Status})->{info.StageId}, "))}.\n\n");

            foreach (var stageExecution in strategyStageExecutions)
            {
                var transitionsPrevStage = await strategyRepository.FetchTransitionByDestinationStage(strategyExecution.StrategyId, stageExecution.StageId);
                if (transitionsPrevStage == null)
                {
                    logger.LogInformation($"Stage execution: {stageExecution.Id}, stage {stageExecution.StageId}, transition null\n\n");
                    var stageInfo = await strategyRepository.FetchStageWithUserByStageId(stageExecution.StageId, stageExecution.Id);
                    logger.LogInformation($"Stage: {stageInfo.Id}, model {stageInfo.StageModel}\n\n");
                    executableNodes.Add(stageInfo);
                    break;
                }
                logger.LogInformation($"Stage execution: {stageExecution.Id}, stage {stageExecution.StageId}, transition {transitionsPrevStage.Id}, sourceStage {transitionsPrevStage.StageSourceId}, destinationStage {transitionsPrevStage.StageDestinationId}\n\n");

                var previousStageExecution = await strategyRepository.FetchStageExecutionByStageId(transitionsPrevStage.StageSourceId, strategyExecution.Id);
                logger.LogInformation($"Previous stage: {previousStageExecution.StageId}, transition {transitionsPrevStage.Id}, sourceStage {transitionsPrevStage.StageSourceId}, destinationStage {transitionsPrevStage.StageDestinationId}\n\n");
                if (previousStageExecution.Status == StageExecutionStatus.Completed || previousStageExecution.Status == StageExecutionStatus.Failed)
                {
                    var stageInfo = await strategyRepository.FetchStageWithUserByStageId(stageExecution.StageId, stageExecution.Id);
                    logger.LogInformation($"Stage: {stageInfo.Id}, model {stageInfo.StageModel}\n\n");
                    executableNodes.Add(stageInfo);
                }
            }
        }

        return executableNodes;
    }

    private async Task FailStage(IStrategyRepository strategyRepository, Guid stageId, Guid strategyId, Guid strategyExecutionId, CancellationToken ct)
    {
        var strategyStages = await strategyRepository.FetchStagesByStrategyId(strategyId, ct);
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

        await strategyRepository.UpdateStageExecutionStatusOnFailedBulk(descendants.ToList(), strategyExecutionId, ct);
        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(strategyExecutionId, ct);
        if (activeExecutions.Count == 0)
        {
            await strategyRepository.UpdateStrategyExecutionStatus(strategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Failed, ct);
        }
    }

    private async Task ProcessPendingNodes(IStrategyRepository strategyRepository, ModelService.ModelServiceClient modelServiceClient, UserService.UserServiceClient userServiceClient, IUnitOfWork unitOfWork, ITokenService tokenService, IAccountRepository accountRepository, CancellationToken ct)
    {
        logger.LogInformation("ProcessPendingNodes started.");

        var nextNodes = await GetExecutableNodes(strategyRepository, ct);
        var nodesGroupedByUsers = nextNodes.GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var nodesGroup in nodesGroupedByUsers)
        {
            var userId = nodesGroup.Key;
            var userNodes = nodesGroup.Value;
            int countNodes = userNodes.Count;
            logger.LogInformation($"User={userId}, countNodes={countNodes}, userNodes={string.Join(", ", userNodes.Select(info => $"{info.Id}"))}");

            var user = await accountRepository.GetUserById(userId);
            var token = await tokenService.GenerateToken(new AccountEntityModel
            {
                Id = userId,
                Email = user!.Email
            });

            var meta = new Metadata { { "Authorization", $"Bearer {token}" } };

            var potfolioInfo = await userServiceClient.GetPortfolioAsync(new Empty(), headers: meta, cancellationToken: ct);
            double initialBalance = potfolioInfo.RubleBalance / countNodes;
            logger.LogInformation($"Balance from python {potfolioInfo.RubleBalance}, initialBalance = {initialBalance}");
            initialBalance = 1;

            foreach (var node in userNodes)
            {
                logger.LogInformation($"node={node.Id}, model={node.StageModel}");
                // TODO: поменять MaxExecutionDurationSeconds
                var request = new StartExecutionRequest
                {
                    ModelId = node.StageModel,
                    InitialBalance = initialBalance,
                    MaxExecutionDurationSeconds = 4
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

                logger.LogInformation($"Node sent for execution. ExecutionId {startExecutionResponse.ExecutionId}");

                await unitOfWork.BeginTransactionAsync();
                try
                {
                    await strategyRepository.SaveExternalExecutionId(node.StageExecutionId, startExecutionResponse.ExecutionId, ct);
                    await strategyRepository.UpdateStageExecutionStatus(node.StageExecutionId, StageExecutionStatus.Running, ct);
                    await strategyRepository.UpdateStrategyExecutionStatus(node.StrategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Running, ct);

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

            var request = new GetExecutionStatusRequest
            {
                ExecutionId = execution.ExternalExecutionId.Value
            };

            var token = await tokenService.GenerateToken(new AccountEntityModel
            {
                Id = execution.UserId,
                Email = execution.Email
            });

            var meta = new Metadata { { "Authorization", $"Bearer {token}" } };
            var response = await modelServiceClient.GetExecutionStatusAsync(request, headers: meta, cancellationToken: ct);

            if (MapExecutionStatus(response.Status) != execution.Status)
            {
                logger.LogInformation($"different statuses");
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    if (response.Status == ExecutionStatus.Failed)
                    {
                        await FailStage(strategyRepository, execution.StageId, execution.StrategyId, execution.StrategyExecutionId, ct);
                    }
                    if (response.Status == ExecutionStatus.Completed)
                    {
                        logger.LogInformation($"Change status to Completed");
                        var activeExecutions = await strategyRepository.FetchActiveStageExecutions(execution.StrategyExecutionId, ct);
                        logger.LogInformation($"Active counts {activeExecutions.Count}");
                        if (activeExecutions.Count == 1)
                        {
                            await strategyRepository.UpdateStrategyExecutionStatus(execution.StrategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus.Completed, ct);
                        }
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

                await ProcessRunningNodes(strategyRepository, modelServiceClient, unitOfWork, tokenService, ct);
                await ProcessPendingNodes(strategyRepository, modelServiceClient, userServiceClient, unitOfWork, tokenService, accountRepository, ct);

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

