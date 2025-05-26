namespace TradeWiseBackend.Domain.ServiceModels;

public enum BacktestStatus
{
    Unknown,
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}
