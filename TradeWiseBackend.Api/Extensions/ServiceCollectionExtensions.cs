using Microsoft.Extensions.DependencyInjection;

namespace TradeWiseBackend.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        return services;
    }
}