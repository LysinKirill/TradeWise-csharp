using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class GetStrategyRequest(
    [property: Required(ErrorMessage = "StrategyId required")] Guid StrategyId
);
