using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class DeleteStrategyRequest(
    [param: Required(ErrorMessage = "StrategyId required")] Guid? StrategyId
);
