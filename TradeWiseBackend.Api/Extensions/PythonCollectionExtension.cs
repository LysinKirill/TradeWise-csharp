using Backtest;
using Grpc.Core;
using Grpc.Net.Client;
using Invest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model;
using User;

namespace TradeWiseBackend.Api.Extensions;

public static class PythonCollectionExtension
{
    public static IServiceCollection AddPythonGrpcClients(this IServiceCollection services,
        IConfiguration configuration, HttpMessageHandler handler)
    {
        services.Configure<PythonBackend.PythonBackend>(configuration.GetSection(nameof(PythonBackend)));
        var pythonBackend = configuration.GetRequiredSection("PythonBackend").Get<PythonBackend.PythonBackend>()!;
        var channel = GrpcChannel.ForAddress(pythonBackend.Url, new GrpcChannelOptions
        {
            HttpHandler = handler,
            // Disable SSL/TLS
            UnsafeUseInsecureChannelCallCredentials = true,
            Credentials = ChannelCredentials.Insecure
        });
        
        services.AddSingleton(new UserService.UserServiceClient(channel));
        services.AddSingleton(new InvestService.InvestServiceClient(channel));
        services.AddSingleton(new ModelService.ModelServiceClient(channel));
        services.AddSingleton(new BacktestService.BacktestServiceClient(channel));

        return services;
    }
}