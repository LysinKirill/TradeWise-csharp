using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record CreateStrategyRequest(
    [property: Required(ErrorMessage = "Title required")] string Title,
    string? Description,
    [property: Required(ErrorMessage = "StrategyStages required")] List<StrategyStage> StrategyStages,
    [property: Required(ErrorMessage = "StrategyTransitions required")] List<StrategyTransition> StrategyTransitions
);