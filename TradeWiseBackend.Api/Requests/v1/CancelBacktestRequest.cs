namespace TradeWiseBackend.Api.Requests.v1;

public record class CancelBacktestRequest(
    Guid BacktestExecutionId
);
