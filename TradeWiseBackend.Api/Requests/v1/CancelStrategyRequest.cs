namespace TradeWiseBackend.Api.Requests.v1;

public record class CancelStrategyRequest(
    Guid StrategyExecutionId
);