namespace TradeWiseBackend.Api.Requests.models;

public record class StrategyStage(
    int Id,
    StrategyStageType StageType,
    StrategyStageModel StageModel
);
