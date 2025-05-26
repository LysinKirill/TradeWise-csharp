using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.models;

public record TransitionCondition(
    [property: Required(ErrorMessage = "TransitionConditionType required")] TransitionConditionType TransitionConditionType,
    [property: Required(ErrorMessage = "StatType required")] StatType StatType,
    [property: Required(ErrorMessage = "Value required")] double Value
);