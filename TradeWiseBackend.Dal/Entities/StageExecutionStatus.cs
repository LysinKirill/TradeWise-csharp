namespace TradeWiseBackend.Dal.Entities;

public enum StageExecutionStatus
{
    // TODO: продумать статусную модель
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
