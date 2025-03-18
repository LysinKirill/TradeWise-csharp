using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Dal;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Api.Extensions;

public static class IdentityCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddAuthorizationBuilder();
        services.AddIdentityCore<AccountEntity>()
            .AddEntityFrameworkStores<DatabaseContext>();
        services.AddIdentityApiEndpoints<AccountEntity>();
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            //options.Password.RequireLowercase = true;
            //options.Password.RequireUppercase = true;
            //options.Password.RequiredLength = 10;
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }
}