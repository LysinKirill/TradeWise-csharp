using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class CancelStrategyRequest(
    [property: Required(ErrorMessage = "StrategyExecutionId required")] Guid StrategyExecutionId
);