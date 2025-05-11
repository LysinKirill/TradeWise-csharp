namespace TradeWiseBackend.Api.Responses.models;

public record class PortfolioPosition(
    string InstrumentId,
    int Quantity,
    string Ticker,
    float DailyYield,
    float CurrentPrice
);
