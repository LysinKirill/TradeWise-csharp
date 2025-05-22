namespace TradeWiseBackend.Domain.RepositoryModels;

public record class StageInfo(
    Guid Id,
    Guid StrategyId,
    long StageModel,
    string UserId,
    long? ExternalExecutionId
);