namespace TradeWiseBackend.Domain.RepositoryModels;

public enum StrategyExecutionStatus
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed
}