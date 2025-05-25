namespace TradeWiseBackend.Domain.RepositoryModels;

public record StageExecutionWithUserInfo(
    Guid Id,
    Guid StageId,
    StageExecutionStatus Status,
    long? ExternalExecutionId,
    string UserId,
    string Email,
    Guid StrategyExecutionId,
    Guid StrategyId
);