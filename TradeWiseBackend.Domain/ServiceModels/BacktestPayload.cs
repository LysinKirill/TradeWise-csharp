namespace TradeWiseBackend.Domain.ServiceModels;

public record class BacktestPayload(
    Guid StrategyId,
    long ModelId,
    DateTime From,
    DateTime To,
    double InitialBalance
);