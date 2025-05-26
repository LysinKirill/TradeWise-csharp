using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Bll.Services;
using TradeWiseBackend.Domain.Interfaces.Services;

namespace TradeWiseBackend.Bll.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        services.AddScoped<IInvestApiService, InvestApiService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IStrategyService, StrategyService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IBacktestService, BacktestService>();
        services.AddScoped<StrategyValidationService>();
        services.AddHostedService<StrategyStatusUpdateWorker>();

        return services;
    }
}