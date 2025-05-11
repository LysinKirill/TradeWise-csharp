namespace TradeWiseBackend.Domain.ServiceModels;

public record class GetCandlesByInstrumentPayload(
    string InstrumentId,
    DateTime From,
    DateTime To
);
