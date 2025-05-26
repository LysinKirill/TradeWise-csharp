using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record class ValidateStrategyRequest(
    [property: Required(ErrorMessage = "StrategyStages required")] List<StrategyStage> StrategyStages,
    [property: Required(ErrorMessage = "StrategyTransitions required")] List<StrategyTransition> StrategyTransitions
);
