namespace TradeWiseBackend.Domain.RepositoryModels;

public record class StageInfoCut(
    Guid Id,
    long ModelId,
    int MaxExecutionDurationSeconds
);