namespace TradeWiseBackend.Api.Responses.models;

public record class ShortModelInfo(
    long? Id,
    string? InstrumentId,
    string? Name,
    string? Type,
    DateTime? CreatedAt
);