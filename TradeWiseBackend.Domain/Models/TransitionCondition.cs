namespace TradeWiseBackend.Domain.Models;

public record TransitionCondition(
    TransitionConditionType TransitionConditionType,
    StatType StatType,
    double Value,
    string InstrumentId
);