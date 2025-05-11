namespace TradeWiseBackend.Domain.Models;

public record class PortfolioPosition(
    string InstrumentId,
    int Quantity,
    string Ticker,
    float DailyYield,
    float CurrentPrice
);