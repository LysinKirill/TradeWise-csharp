using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IInvestApiService
{
    Task<GrpcCallResult> LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountPayload userRegistrationPayload, CancellationToken ct);
    Task<List<InstrumentInfo>> GetSupportedInstruments(CancellationToken ct);
}