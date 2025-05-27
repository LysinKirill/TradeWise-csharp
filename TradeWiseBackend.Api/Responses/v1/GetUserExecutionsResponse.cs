using TradeWiseBackend.Api.Responses.models;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetUserExecutionsResponse(
    List<StrategyExecution> Executions
);