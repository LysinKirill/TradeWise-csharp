using Model;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetExecutionOverviewResponse(
    Guid StrategyId, // 1
    double TotalInputAmount, // суммма
    List<string> Instruments, // массив
    DateTime? StartedAt, // min
    DateTime? FinishedAt, // max если стратегия завершена
    int SharesOwned, // сумм
    bool IsPaperTrade, // 1
    ExecutionStatus Status // 1
);
