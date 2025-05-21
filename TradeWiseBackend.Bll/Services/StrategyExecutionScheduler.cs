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

public class StrategyExecutionScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StrategyExecutionScheduler> _logger;
    private readonly ModelService.ModelServiceClient _modelServiceClient;
    private readonly IStrategyRepository _strategyRepository;
    private readonly UserService.UserServiceClient _userServiceClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    private Metadata AuthMetadata
    {
        get
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (token is null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "No authorization header provided"));
            return new Metadata
            {
                { "Authorization", token }
            };
        }
    }

    public StrategyExecutionScheduler(
        IServiceProvider serviceProvider,
        ILogger<StrategyExecutionScheduler> logger,
        ModelService.ModelServiceClient modelServiceClient,
        IStrategyRepository strategyRepository,
        UserService.UserServiceClient userServiceClient,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _modelServiceClient = modelServiceClient;
        _strategyRepository = strategyRepository;
        _userServiceClient = userServiceClient;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    private async Task<List<StageInfo>> GetExecutableNodes(CancellationToken ct)
    {
        var activeStrategyExecutions = await _strategyRepository.GetPendingAndRunningStrategies();
        _logger.LogInformation($"Active strategy executions started StrategyId->StrategyExecutionId:\n{string.Join(",", activeStrategyExecutions.Select(info => $"({info.StrategyId}->{info.Id}), "))}.\n\n");
        var executableNodes = new List<StageInfo>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var strategyStageExecutions = await _strategyRepository.GetPendingStageExecutionsByStrategy(strategyExecution.StrategyId);
            _logger.LogInformation($"Active stage executions started StageExecutionId->StageId->Status:\n{string.Join(", ", strategyStageExecutions.Select(info => $"({info.Id}->{info.Status})->{info.StageId}, "))}.\n\n");

            foreach (var stageExecution in strategyStageExecutions)
            {
                var transitionsPrevStage = await _strategyRepository.FetchTransitionByDestinationStage(strategyExecution.StrategyId, stageExecution.StageId);
                if (transitionsPrevStage == null)
                {
                    _logger.LogInformation($"Stage execution: {stageExecution.Id}, stage {stageExecution.StageId}, transition null\n\n");
                    var stageInfo = await _strategyRepository.FetchStageWithUserByStageId(stageExecution.StageId);
                    _logger.LogInformation($"Stage: {stageInfo.Id}, model {stageInfo.StageModel}\n\n");
                    executableNodes.Add(stageInfo);
                    break;
                }
                _logger.LogInformation($"Stage execution: {stageExecution.Id}, stage {stageExecution.StageId}, transition {transitionsPrevStage.Id}, sourceStage {transitionsPrevStage.StageSourceId}, destinationStage {transitionsPrevStage.StageDestinationId}\n\n");

                var previousStageExecution = await _strategyRepository.FetchStageExecutionByStageId(transitionsPrevStage.StageSourceId);
                _logger.LogInformation($"Previous stage: {previousStageExecution.StageId}, transition {transitionsPrevStage.Id}, sourceStage {transitionsPrevStage.StageSourceId}, destinationStage {transitionsPrevStage.StageDestinationId}\n\n");
                if (previousStageExecution.Status == StageExecutionStatus.Completed || previousStageExecution.Status == StageExecutionStatus.Failed)
                {
                    var stageInfo = await _strategyRepository.FetchStageWithUserByStageId(stageExecution.StageId);
                    _logger.LogInformation($"Stage: {stageInfo.Id}, model {stageInfo.StageModel}\n\n");
                    executableNodes.Add(stageInfo);
                }
            }
        }

        return executableNodes;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("StrategyExecutionScheduler started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var nextNodes = await GetExecutableNodes(ct);
                var nodesGroupedByUsers = nextNodes.GroupBy(s => s.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var nodesGroup in nodesGroupedByUsers)
                {
                    var userId = nodesGroup.Key;
                    var userNodes = nodesGroup.Value;
                    int countNodes = userNodes.Count;
                    _logger.LogInformation($"User={userId}, countNodes={countNodes}, userNodes={string.Join(", ", userNodes.Select(info => $"{info.Id}"))}");

                    var potfolioInfo = await _userServiceClient.GetPortfolioAsync(new Empty(), headers: AuthMetadata, cancellationToken: ct);
                    double initialBalance = potfolioInfo.RubleBalance / countNodes;
                    _logger.LogInformation($"Balance from python {potfolioInfo.RubleBalance}, initialBalance = {initialBalance}");

                    foreach (var node in userNodes)
                    {
                        _logger.LogInformation($"node={node.Id}, model={node.StageModel}");
                        // TODO: поменять MaxExecutionDurationSeconds
                        var request = new StartExecutionRequest
                        {
                            ModelId = node.StageModel,
                            InitialBalance = initialBalance,
                            MaxExecutionDurationSeconds = 300
                        };
                        var startExecutionResponse = await _modelServiceClient.StartExecutionAsync(request, headers: AuthMetadata, cancellationToken: ct);
                        _logger.LogInformation($"Node sent for execution. Status {startExecutionResponse.ExecutionId}");

                        await _unitOfWork.BeginTransactionAsync();
                        try
                        {
                            await _strategyRepository.UpdateStrategyExecutionStatusToRunning(node.Id, node.StrategyId, ct);
                            await _strategyRepository.UpdateStageExecutionStatusToRunning(node.Id, node.StrategyId, ct);

                            await _unitOfWork.CommitAsync();
                        }
                        catch
                        {
                            await _unitOfWork.RollbackAsync();
                            throw;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scheduling strategy executions.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }

        _logger.LogInformation("StrategyExecutionScheduler stopping.");
    }
}

