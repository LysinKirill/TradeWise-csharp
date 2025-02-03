using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Dal.Repositories;
using TradeWiseBackend.Domain.Interfaces.Interfaces.Repositories;
using DbContext = TradeWiseBackend.Dal.DatabaseSettings.DbContext;

namespace TradeWiseBackend.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountsRepository, AccountsRepository>();

        return services;
    }
    
    public static IServiceCollection AddDalInfrastructure(this IServiceCollection services, DatabaseSettings.DbSettings config)
    {
        services.AddDbContext<DbContext>(options =>
            options.UseNpgsql(config.ConnectionString));
        return services;
    }
}