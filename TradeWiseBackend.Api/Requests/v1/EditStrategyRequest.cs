using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record class EditStrategyRequest(
    [param: Required(ErrorMessage = "StrategyId required")] Guid? StrategyId,
    [param: Required(ErrorMessage = "Title required")] string? Title,
    string? Description,
    [param: Required(ErrorMessage = "StrategyStages required")] List<StrategyStage> StrategyStages,
    [param: Required(ErrorMessage = "StrategyTransitions required")] List<StrategyTransition> StrategyTransitions,
    [param: Required(ErrorMessage = "AllocatedBudget required")] double? AllocatedBudget
);
