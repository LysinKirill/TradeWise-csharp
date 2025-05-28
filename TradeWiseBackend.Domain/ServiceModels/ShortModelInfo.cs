namespace TradeWiseBackend.Domain.ServiceModels;

public record class ShortModelInfo(
    long? Id,
    string? InstrumentId,
    string? Name,
    string? Type,
    DateTime? CreatedAt
);