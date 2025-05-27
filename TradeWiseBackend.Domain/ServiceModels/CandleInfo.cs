namespace TradeWiseBackend.Domain.ServiceModels;

public record class CandleInfo(
    DateTime Timestamp,
    float Open,
    float High,
    float Low,
    float Close
);