using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record class ValidateStrategyRequest(
    [param: Required(ErrorMessage = "StrategyStages required")] List<StrategyStage> StrategyStages,
    [param: Required(ErrorMessage = "StrategyTransitions required")] List<StrategyTransition> StrategyTransitions
);
