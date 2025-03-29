using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IInvestApiService
{
    Task LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountPayload userRegistrationPayload, CancellationToken ct);
}