namespace TradeWiseBackend.Api.Requests.models;

public record class StrategyTransition(
    int SourceStageId,
    int DestinationStageId,
    List<TransitionCondition>? TransitionConditions
);