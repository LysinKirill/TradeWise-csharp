namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyExecutionModel(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    StrategyExecutionStatus Status,
    Guid StrategyId,
    bool IsPaperTrade,
    double AllocatedBudget
);