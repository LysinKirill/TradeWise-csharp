using System;

namespace TradeWiseBackend.Domain.RepositoryModels;

public record StageExecutionModel
(
    Guid Id,
    Guid StageId,
    Guid ExecutionId,
    Guid StrategyExecutionId,
    StageExecutionStatus Status,
    long? ExternalExecutionId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
