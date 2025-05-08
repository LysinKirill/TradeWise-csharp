namespace TradeWiseBackend.Domain.Models;

public record StrategyStage(
    Guid Id,
    StrategyStageType StageType,
    string? StageModel
);