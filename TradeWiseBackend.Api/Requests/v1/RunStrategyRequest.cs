using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class RunStrategyRequest(
    [param: Required(ErrorMessage = "StrategyId required")] Guid? StrategyId,
    [param: Required(ErrorMessage = "IsPaperTrade required")] bool? IsPaperTrade,
    [param: Required(ErrorMessage = "AllocatedBudget required")] double? AllocatedBudget
);