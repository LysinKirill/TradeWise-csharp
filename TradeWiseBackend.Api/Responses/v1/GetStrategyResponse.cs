using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetStrategyResponse(
    string Title,
    string? Description,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);
