using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Repositories;
using TradeWiseBackend.Dal.Transactions;
using TradeWiseBackend.Domain.Interfaces.Repositories;

namespace TradeWiseBackend.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStrategyRepository, StrategyRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBacktestRepository, BacktestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection AddDalInfrastructure(this IServiceCollection services, DbSettings config)
    {
        services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(config.ConnectionString));
        return services;
    }
}