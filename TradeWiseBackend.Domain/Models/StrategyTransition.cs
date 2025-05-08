namespace TradeWiseBackend.Api.Requests.models;

public record StrategyTransition(
    int SourceStageId,
    int DestinationStageId,
    List<TransitionCondition> TransitionConditions
);