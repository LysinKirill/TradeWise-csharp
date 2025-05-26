namespace TradeWiseBackend.Domain.ServiceModels;

public enum BacktestStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}
