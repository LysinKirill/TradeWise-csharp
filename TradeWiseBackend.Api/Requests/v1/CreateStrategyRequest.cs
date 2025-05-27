using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record CreateStrategyRequest(
    [param: Required(ErrorMessage = "Title required")]
    string? Title,
    string? Description,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);