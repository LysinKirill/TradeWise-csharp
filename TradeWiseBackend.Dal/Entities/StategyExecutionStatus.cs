namespace TradeWiseBackend.Dal.Entities;

public enum StrategyExecutionStatus
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed
}