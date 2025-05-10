using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record class ValidateStrategyRequest(
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);
