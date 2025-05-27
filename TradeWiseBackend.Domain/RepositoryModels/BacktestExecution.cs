namespace TradeWiseBackend.Domain.RepositoryModels;

public record class BacktestExecution(
    Guid Id,
    long ExternalExecutionId,
    string UserId
);