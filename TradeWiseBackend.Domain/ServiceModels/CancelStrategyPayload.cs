namespace TradeWiseBackend.Domain.ServiceModels;

public record class CancelStrategyPayload(
    Guid StrategyExecutionId
);