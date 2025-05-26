namespace TradeWiseBackend.Domain.ServiceModels;

public record class RunBacktestPayload(
    long ModelId,
    DateTime From,
    DateTime To,
    double InitialBalance,
    string UserId
);
