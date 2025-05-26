using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record LinkInvestApiKeyWithAccountRequest(
    [property: Required(ErrorMessage = "InvestApiKey required")] string InvestApiKey
);