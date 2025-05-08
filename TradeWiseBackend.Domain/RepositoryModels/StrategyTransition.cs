using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyTransition(
    Guid StrategyTransitionId,
    Guid? StageSourceId,
    Guid? StageDestinationId,
    StatType StatType,
    TransitionConditionType Operation,
    double Value
);