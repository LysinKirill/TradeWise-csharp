namespace TradeWiseBackend.Domain.RepositoryModels;

public record class StageExecutionInfo(
    Guid Id,
    StageExecutionStatus Status
);
