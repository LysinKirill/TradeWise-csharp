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

public class StrategyExecutionScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StrategyExecutionScheduler> _logger;
    private readonly ModelService.ModelServiceClient _modelServiceClient;
    private readonly IStrategyRepository _strategyRepository;

    public StrategyExecutionScheduler(
        IServiceProvider serviceProvider,
        ILogger<StrategyExecutionScheduler> logger,
        ModelService.ModelServiceClient modelServiceClient,
        IStrategyRepository strategyRepository)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _modelServiceClient = modelServiceClient;
        _modelServiceClient = modelServiceClient;
        _strategyRepository = strategyRepository;
    }

    private async Task<List<StageExecutionInfo>> GetExecutableNodes(CancellationToken ct)
    {
        var activeStrategyExecutions = await _strategyRepository.GetPendingAndRunningStrategies();
        var executableNodes = new List<StageExecutionInfo>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var strategyNodes = await _strategyRepository.GetPendingAndRunningStageExecutionsByStrategy(strategyExecution.StrategyId);

            foreach (var node in strategyNodes)
            {
                var transitionsPrevStages = await _strategyRepository.FetchTransitionByDestinationStage(strategyExecution.StrategyId, node.Id);
                if (transitionsPrevStages == null)
                {
                    executableNodes.Add(node);
                    break;
                }

                var previousStageExecution = await _strategyRepository.FetchStageExecutionById(transitionsPrevStages.StageSourceId);
                // проверить все терминальные статусы
                if (previousStageExecution.Status == StageExecutionStatus.Completed)
                {
                    executableNodes.Add(node);
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

                if (nextNodes.Count != 0)
                {
                    _logger.LogInformation($"Found nodes to execute.");

                    foreach (var node in nextNodes)
                    {
                        _logger.LogInformation($"node={node.Id}, strategy={node.Status}, ");
                        // отправить ноды
                    }

                    _logger.LogInformation($"Nodes sent for execution.");
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

