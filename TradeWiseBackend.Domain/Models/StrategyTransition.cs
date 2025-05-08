namespace TradeWiseBackend.Domain.Models;

public record StrategyTransition(
    Guid? SourceStageId,
    Guid? DestinationStageId,
    List<TransitionCondition> TransitionConditions
);