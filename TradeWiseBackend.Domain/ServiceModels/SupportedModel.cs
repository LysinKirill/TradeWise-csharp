namespace TradeWiseBackend.Domain.ServiceModels;

public record class SupportedModel(
    long Id,
    string InstrumentId,
    string Name,
    string Type,
    DateTime CreatedAt
);