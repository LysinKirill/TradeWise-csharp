using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record class CreateStrategyRequest(
    string? Title,
    string? Description,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);
