using System;

namespace TradeWiseBackend.Domain.RepositoryModels;

public record StageExecutionModel
(
    Guid Id,
    Guid StageId,
    Guid StrategyExecutionId,
    StageExecutionStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
