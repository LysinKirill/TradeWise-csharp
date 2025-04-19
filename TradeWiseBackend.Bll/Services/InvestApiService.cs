using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;
using User;

namespace TradeWiseBackend.Bll.Services;

public class InvestApiService(
    UserService.UserServiceClient userServiceClient,
    InvestService.InvestServiceClient investServiceClient,
    IHttpContextAccessor httpContextAccessor
) : IInvestApiService
{
    private Metadata AuthMetadata
    {
        get
        {
            var token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (token is null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "No authorization header provided"));
            return new Metadata
            {
                { "Authorization", token }
            };
        }
    }

    public async Task LinkInvestApiKeyWithAccount(
        LinkInvestApiKeyWithAccountPayload linkInvestApiKeyWithAccountPayload,
        CancellationToken ct)
    {
        var request = new AddInvestApiKeyRequest { ApiKey = linkInvestApiKeyWithAccountPayload.InvestApiKey };

        await userServiceClient.AddInvestApiKeyAsync(request, headers: AuthMetadata, cancellationToken: ct);

        //TODO: возвращать результат
    }

    public async Task<List<Domain.Models.InstrumentInfo>> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList = await investServiceClient.GetSupportedInstrumentsAsync(new Empty(), headers: AuthMetadata, cancellationToken: ct);

        var instruments = instrumentsList.Instruments.Select(protoInstrument => new Domain.Models.InstrumentInfo
        {
            id = protoInstrument.Id,
            figi = protoInstrument.Figi ?? string.Empty,
            name = protoInstrument.Name,
            lot = protoInstrument.Lot,
            currency = protoInstrument.Currency,
            sector = protoInstrument.Sector,
            buy_available = protoInstrument.BuyAvailable,
            sell_available = protoInstrument.SellAvailable
        }).ToList();

        return instruments;
    }
}