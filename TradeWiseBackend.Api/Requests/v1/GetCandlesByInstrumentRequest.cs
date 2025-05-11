namespace TradeWiseBackend.Api.Requests.v1;

public record class GetCandlesByInstrumentRequest(
    string InstrumentId,
    DateTime From,
    DateTime To
);
