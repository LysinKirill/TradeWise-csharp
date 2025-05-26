namespace TradeWiseBackend.Api.Responses.models;

public record class BacktestInfo(
    long BacktestId,
    DateTime StartedAt,
    DateTime FinishedAt,
    DateTime TestPeriodStart,
    DateTime TestPeriodEnd,
    BacktestStatus Status,
    double Profit,
    int TradesCount,
    double InitialBalance,
    double FinalBalance,
    DateTime CreatedAt
);
