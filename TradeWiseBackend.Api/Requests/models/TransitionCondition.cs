using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.models;

public record TransitionCondition(
    [param: Required(ErrorMessage = "TransitionConditionType required")] TransitionConditionType? TransitionConditionType,
    [param: Required(ErrorMessage = "StatType required")] StatType? StatType,
    [param: Required(ErrorMessage = "Value required")] double? Value
);