using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class ExecutionInfo(
    Guid StrategyId,
    double TotalInputAmount,
    List<string> Instruments,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int SharesOwned,
    bool IsPaperTrade,
    StrategyExecutionStatus Status
);