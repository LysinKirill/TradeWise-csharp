using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class GetCandlesByInstrumentRequest(
    [property: Required(ErrorMessage = "InstrumentId required")] string InstrumentId,
    [property: Required(ErrorMessage = "From required")] DateTime From,
    [property: Required(ErrorMessage = "To required")] DateTime To
);
