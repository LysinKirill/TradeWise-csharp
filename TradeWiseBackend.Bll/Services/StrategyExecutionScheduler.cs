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

    private async Task<List<StageExecutionEntity>> GetExecutableNodesAsync(DatabaseContext dbContext, CancellationToken ct)
    {

        var activeStrategyExecutions = await _strategyRepository.GetActiveStrategies();
        var executableNodes = new List<StageExecutionEntity>();

        foreach (var strategyExecution in activeStrategyExecutions)
        {
            var nodes = await _strategyRepository.GetStagesByStrategy(strategyExecution.Id);

            var nextNode = nodes
                .Where(n => n.Status == ExecutionStatus.Pending)
                .FirstOrDefault(n =>
                {
                    if (n.PreviousStageExecutionId == null)
                        return true; // Начальная нода

                    var prev = nodes.FirstOrDefault(p => p.Id == n.PreviousStageExecutionId);
                    return prev != null && prev.Status == ExecutionStatus.Completed;
                });

            if (nextNode != null)
            {
                executableNodes.Add(nextNode);
            }
        }

        return executableNodes;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StrategyExecutionScheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // получить следующую ноду для запуска
                string nextNode = null;

                if (nextNode != null)
                {
                    _logger.LogInformation($"Found node {nextNode} to execute.");

                    // Отправляем на исполнение
                    // меняем статус и updated_at

                    _logger.LogInformation($"Node {nextNode} sent for execution.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scheduling strategy executions.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("StrategyExecutionScheduler stopping.");
    }
}

