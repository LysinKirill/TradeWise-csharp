namespace TradeWiseBackend.Domain.RepositoryModels;

public record class StageExecutionInfo(
    Guid Id,
    Guid StageId,
    StageExecutionStatus Status,
    long? ExternalExecutionId
);
