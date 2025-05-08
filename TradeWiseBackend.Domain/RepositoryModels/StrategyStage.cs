namespace TradeWiseBackend.Domain.RepositoryModels;

public record class StrategyStage(
    Guid StageId,
    string ModelName,
    Account User
);
