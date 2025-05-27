namespace TradeWiseBackend.Dal.Entities;

public enum StageExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Aborted
}