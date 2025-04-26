using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record GetInstrumentStatPayload(
    string InstrumentId,
    StatType StatType,
    DateTime From,
    DateTime To
);