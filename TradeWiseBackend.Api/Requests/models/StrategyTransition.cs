namespace TradeWiseBackend.Api.Requests.models;

public record StrategyTransition(
    Guid? SourceStageId,
    Guid? DestinationStageId,
    List<TransitionCondition> TransitionConditions
);