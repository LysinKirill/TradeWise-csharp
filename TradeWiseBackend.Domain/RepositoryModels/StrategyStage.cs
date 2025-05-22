namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyStage(
    Guid Id,
    Guid StrategyId,
    long StageModel
);