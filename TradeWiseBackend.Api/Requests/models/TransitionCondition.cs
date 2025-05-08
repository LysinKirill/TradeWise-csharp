using System;

namespace TradeWiseBackend.Api.Requests.models;

public record TransitionCondition
(
    TransitionConditionType TransitionConditionType,
    StatType StatType,
    double Value
);
