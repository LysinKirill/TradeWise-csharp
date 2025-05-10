namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyStage(
    Guid StageId,
    Guid StrategyId,
    string? ModelName
);