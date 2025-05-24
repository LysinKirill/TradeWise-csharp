using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TradeWiseBackend.Api.Responses.models;

public record class Model(
    long Id,
    string InstrumentId,
    string Name,
    string Type,
    DateTime CreatedAt
);
