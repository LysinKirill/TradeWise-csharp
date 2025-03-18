using Grpc.Net.Client;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class InvestApiService : IInvestApiService
{
    public async Task LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountPayload linkInvestApiKeyWithAccountPayload)
    {
        var address = "https://localhost/50001";
        using var channel = GrpcChannel.ForAddress(address);
        var client = new InvestService.InvestServiceClient(channel);
        var request = new AddInvestApiKeyRequest { ApiKey = linkInvestApiKeyWithAccountPayload.InvestApiKey };
        try
        {
            await client.AddInvestApiKeyAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Some error occured: {ex.Message}");
        }
    }
}