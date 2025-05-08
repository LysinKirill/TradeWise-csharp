namespace TradeWiseBackend.Api.Requests.models;

public record StrategyStage(
    Guid Id,
    StrategyStageType StageType,
    string? StageModel
);