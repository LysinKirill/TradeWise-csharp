using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.models;

public record StrategyTransition(
    Guid? SourceStageId,
    Guid? DestinationStageId,
    [property: Required(ErrorMessage = "TransitionConditions required")] List<TransitionCondition> TransitionConditions
);