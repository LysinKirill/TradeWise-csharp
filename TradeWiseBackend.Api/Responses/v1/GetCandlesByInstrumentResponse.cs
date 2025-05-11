using TradeWiseBackend.Api.Responses.models;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetCandlesByInstrumentResponse(
    List<Candle> Candles
);
