namespace TradeWiseBackend.Domain.RepositoryModels;

public record StrategyStage(
    Guid StageId,
    string? ModelName,
    Account User
);