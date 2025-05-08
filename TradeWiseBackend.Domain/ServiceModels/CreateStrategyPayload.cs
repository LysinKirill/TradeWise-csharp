using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record CreateStrategyPayload(
    string? Title,
    string? Description,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions,
    string UserId
);
