using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class InvestApiService(
    UserService.UserServiceClient userServiceClient,
    Invest.InvestService.InvestServiceClient investServiceClient,
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
    }

    public async Task<List<InstrumentInfo>> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList =
            await investServiceClient.GetSupportedInstrumentsAsync(new Empty(), headers: AuthMetadata,
                cancellationToken: ct);

        return instrumentsList.Instruments.Adapt<List<InstrumentInfo>>();
    }

    public async Task<InstrumentStat> GetInstrumentStat(GetInstrumentStatPayload payload, CancellationToken ct)
    {
        var statType = payload.StatType switch
        {
            StatType.BollingerBandLower => UserStatType.BollingerBandLower,
            StatType.BollingerBandMiddle => UserStatType.BollingerBandMiddle,
            StatType.BollingerBandUpper => UserStatType.BollingerBandUpper,
            StatType.ExponentialMovingAverage => UserStatType.ExponentialMovingAverage,
            StatType.MovingAverage => UserStatType.MovingAverage,
            StatType.MovingAverageConvergenceDivergence => UserStatType.MovingAverageConvergenceDivergence,
            StatType.RelativeStrengthIndex => UserStatType.RelativeStrengthIndex,
            _ => UserStatType.Unknown
        };
        var request = new Invest.GetInstrumentStatRequest
        {
            InstrumentId = payload.InstrumentId, StatType = statType,
            From = Timestamp.FromDateTime(payload.From.ToUniversalTime()),
            To = Timestamp.FromDateTime(payload.To.ToUniversalTime())
        };
        var instrumentStat =
            await investServiceClient.GetInstrumentStatAsync(request, headers: AuthMetadata, cancellationToken: ct);
        return instrumentStat.Adapt<InstrumentStat>();
    }
}