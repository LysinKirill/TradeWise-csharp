using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record LinkInvestApiKeyWithAccountRequest(
    [param: Required(ErrorMessage = "InvestApiKey required")] string? InvestApiKey
);