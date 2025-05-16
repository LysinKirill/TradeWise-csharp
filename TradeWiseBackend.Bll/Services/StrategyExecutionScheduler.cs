using System;

namespace TradeWiseBackend.Bll.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

public class StrategyExecutionScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StrategyExecutionScheduler> _logger;

    public StrategyExecutionScheduler(
        IServiceProvider serviceProvider,
        ILogger<StrategyExecutionScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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

