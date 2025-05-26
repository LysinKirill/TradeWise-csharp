using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class RunStrategyRequest(
   [property: Required(ErrorMessage = "StrategyId required")] Guid StrategyId
);