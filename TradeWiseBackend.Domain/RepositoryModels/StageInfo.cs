namespace TradeWiseBackend.Domain.RepositoryModels;

public record class InfoForExecution(
    Guid Id,
    Guid StrategyId,
    long StageModel,
    string UserId,
    long? ExternalExecutionId,
    Guid StageExecutionId,
    Guid StrategyExecutionId,
    bool IsPaperTrade,
    int MaxExecutionDurationSeconds,
    double AllocatedBudget
);