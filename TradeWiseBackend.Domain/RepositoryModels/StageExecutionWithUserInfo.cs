namespace TradeWiseBackend.Domain.RepositoryModels;

public record StageExecutionWithUserId(
    Guid Id,
    Guid StageId,
    StageExecutionStatus Status,
    long? ExternalExecutionId,
    string UserId,
    Guid StrategyExecutionId,
    Guid StrategyId
);