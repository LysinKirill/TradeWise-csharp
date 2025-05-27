namespace TradeWiseBackend.Api.Responses.models;

public record class Candle(
    DateTime Timestamp,
    float Open,
    float High,
    float Low,
    float Close
);