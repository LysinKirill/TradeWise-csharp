using TradeWiseBackend.Api.Models;

namespace TradeWiseBackend.Api.Responses;

public record GetSupportedInstrumentsResponse(
    List<InstrumentInfo> instruments
);