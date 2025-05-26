using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class GetCandlesByInstrumentRequest(
    [param: Required(ErrorMessage = "InstrumentId required")] string? InstrumentId,
    [param: Required(ErrorMessage = "From required")] DateTime? From,
    [param: Required(ErrorMessage = "To required")] DateTime? To
);
