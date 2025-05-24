using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using Model;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;
using User;

namespace TradeWiseBackend.Bll.Services;

public class InvestApiService(
    UserService.UserServiceClient userServiceClient,
    Invest.InvestService.InvestServiceClient investServiceClient,
    ModelService.ModelServiceClient modelServiceClient,
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
            StatType.BollingerBandLower => Invest.StatType.BollingerBandLower,
            StatType.BollingerBandMiddle => Invest.StatType.BollingerBandMiddle,
            StatType.BollingerBandUpper => Invest.StatType.BollingerBandUpper,
            StatType.ExponentialMovingAverage => Invest.StatType.ExponentialMovingAverage,
            StatType.MovingAverage => Invest.StatType.MovingAverage,
            StatType.MovingAverageConvergenceDivergence => Invest.StatType.MovingAverageConvergenceDivergence,
            StatType.RelativeStrengthIndex => Invest.StatType.RelativeStrengthIndex,
            _ => Invest.StatType.Unknown
        };
        var request = new Invest.GetInstrumentStatRequest
        {
            InstrumentId = payload.InstrumentId,
            StatType = statType,
            From = Timestamp.FromDateTime(payload.From.ToUniversalTime()),
            To = Timestamp.FromDateTime(payload.To.ToUniversalTime())
        };
        var instrumentStat =
            await investServiceClient.GetInstrumentStatAsync(request, headers: AuthMetadata, cancellationToken: ct);
        return instrumentStat.Adapt<InstrumentStat>();
    }

    public async Task<List<CandleInfo>> GetCandlesByInstrument(GetCandlesByInstrumentPayload payload, CancellationToken ct)
    {
        var request = new Invest.GetCandlesRequest
        {
            InstrumentId = payload.InstrumentId,
            From = Timestamp.FromDateTime(payload.From.ToUniversalTime()),
            To = Timestamp.FromDateTime(payload.To.ToUniversalTime())
        };

        var candles =
            await investServiceClient.GetCandlesAsync(request, headers: AuthMetadata, cancellationToken: ct);

        return candles.Candles.Adapt<List<CandleInfo>>();
    }

    public async Task<List<SupportedModel>> GetSupportedModels(CancellationToken ct)
    {
        var models =
            await modelServiceClient.GetAllModelsAsync(new Empty(), headers: AuthMetadata, cancellationToken: ct);

        return models.Models.Adapt<List<SupportedModel>>();
    }
}