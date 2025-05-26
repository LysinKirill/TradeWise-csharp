using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class CancelStrategyRequest(
    [param: Required(ErrorMessage = "StrategyExecutionId required")] Guid? StrategyExecutionId
);