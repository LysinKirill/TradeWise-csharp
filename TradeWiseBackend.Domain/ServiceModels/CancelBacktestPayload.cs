namespace TradeWiseBackend.Domain.ServiceModels;

public record class CancelBacktestPayload(
    Guid BacktestExecutionId
);