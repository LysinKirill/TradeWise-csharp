using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyTransition(
    Guid Id,
    Guid? StageSourceId,
    Guid? StageDestinationId,
    Guid StrategyId,
    StatType StatType,
    TransitionConditionType Operation,
    double Value
);