namespace TradeWiseBackend.Api.Responses.models;

public record class StrategyExecution(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    StrategyExecutionStatus Status,
    Guid StrategyId
);