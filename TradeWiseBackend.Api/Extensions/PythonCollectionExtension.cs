using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User;

namespace TradeWiseBackend.Api.Extensions;

public static class PythonCollectionExtension
{
    public static IServiceCollection AddPythonGrpcClients(this IServiceCollection services, IConfiguration configuration, HttpMessageHandler handler)
    {
        services.Configure<PythonBackend.PythonBackend>(configuration.GetSection(nameof(PythonBackend)));
        var python_backend = configuration.GetRequiredSection("PythonBackend").Get<PythonBackend.PythonBackend>()!;
        services.AddGrpcClient<UserService.UserServiceClient>(options =>
            {
                options.Address = new Uri(python_backend.Url);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddGrpcClient<Invest.InvestService.InvestServiceClient>(options =>
            {
                options.Address = new Uri(python_backend.Url);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddGrpcClient<Model.ModelService.ModelServiceClient>(options =>
            {
                options.Address = new Uri(python_backend.Url);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddGrpcClient<Backtest.BacktestService.BacktestServiceClient>(options =>
            {
                options.Address = new Uri(python_backend.Url);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services;
    }
}
