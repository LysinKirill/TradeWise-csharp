namespace TradeWiseBackend.Api.Requests.models;

public record StrategyStage(
    int Id,
    StrategyStageType StageType,
    StrategyStageModel StageModel
);
