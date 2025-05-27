namespace TradeWiseBackend.Domain.ServiceModels;

public record class RunStrategyPayload(
    Guid StrategyId,
    bool IsPaperTrade,
    double AllocatedBudget
);