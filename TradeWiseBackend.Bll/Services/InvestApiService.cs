using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;
using User;

using ApiStatType = TradeWiseBackend.Api.Requests.models.StatType;
using UserStatType = User.StatType;

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

    public async Task<GrpcCallResult> LinkInvestApiKeyWithAccount(
        LinkInvestApiKeyWithAccountPayload linkInvestApiKeyWithAccountPayload,
        CancellationToken ct)
    {
        var request = new AddInvestApiKeyRequest { ApiKey = linkInvestApiKeyWithAccountPayload.InvestApiKey };

        try
        {
            var response = await userServiceClient.AddInvestApiKeyAsync(request, headers: AuthMetadata, cancellationToken: ct);
            return GrpcCallResult.Success();
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"Exception in add-invest-api-key: status = {ex.Status.StatusCode}, details = {ex.Status.Detail}");
            return GrpcCallResult.Fail(ex.Status.StatusCode, ex.Status.Detail);
        }
    }

    public async Task<List<Domain.Models.InstrumentInfo>> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList = await investServiceClient.GetSupportedInstrumentsAsync(new Empty(), headers: AuthMetadata, cancellationToken: ct);

        return instrumentsList.Instruments.Adapt<List<Domain.Models.InstrumentInfo>>();
    }

    public async Task<InstrumentStat> GetInstrumentStat(GetInstrumentStatPayload payload, CancellationToken ct)
    {

        // var s = System.Enum.TryParse<User.StatType>(payload.StatType.ToString(), out var parsed)
        // ? parsed
        // : User.StatType.Unknown;
        var statType = payload.StatType switch
        {
            ApiStatType.BollingerBandLower => UserStatType.BollingerBandLower,
            ApiStatType.BollingerBandMiddle => UserStatType.BollingerBandMiddle,
            ApiStatType.BollingerBandUpper => UserStatType.BollingerBandUpper,
            ApiStatType.ExponentialMovingAverage => UserStatType.ExponentialMovingAverage,
            ApiStatType.MovingAverage => UserStatType.MovingAverage,
            ApiStatType.MovingAverageConvergenceDivergence => UserStatType.MovingAverageConvergenceDivergence,
            ApiStatType.RelativeStrengthIndex => UserStatType.RelativeStrengthIndex,
            _ => UserStatType.Unknown
        };
        var request = new GetInstrumentStatRequest { InstrumentId = payload.InstrumentId, StatType = statType, From = Timestamp.FromDateTime(payload.From), To = Timestamp.FromDateTime(payload.To) };
        var instrumentStat = await investServiceClient.GetInstrumentStatAsync(request, headers: AuthMetadata, cancellationToken: ct);

        return instrumentStat.Adapt<InstrumentStat>();
    }
}