using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Repositories;
using TradeWiseBackend.Domain.Interfaces.Repositories;

namespace TradeWiseBackend.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountsRepository, AccountsRepository>();

        return services;
    }

    public static IServiceCollection AddDalInfrastructure(this IServiceCollection services, DbSettings config)
    {
        Console.WriteLine(config.ConnectionString);
        services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(config.ConnectionString));
        return services;
    }
}