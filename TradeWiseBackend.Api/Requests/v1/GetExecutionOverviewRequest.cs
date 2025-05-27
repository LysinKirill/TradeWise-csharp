namespace TradeWiseBackend.Api.Requests.v1;

public record class GetExecutionOverviewRequest(
    Guid StrategyExecutionId
);
