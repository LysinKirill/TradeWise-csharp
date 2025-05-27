using Model;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetExecutionOverviewResponse(
    Guid StrategyId,
    double TotalInputAmount,
    List<string> Instruments,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int SharesOwned,
    bool IsPaperTrade,
    ExecutionStatus Status
);