using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Dal;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Api.Extensions;

public static class IdentityCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddIdentityCore<AccountEntity>()
            .AddEntityFrameworkStores<DatabaseContext>();
        services.AddIdentityApiEndpoints<AccountEntity>();

        return services;
    }
}