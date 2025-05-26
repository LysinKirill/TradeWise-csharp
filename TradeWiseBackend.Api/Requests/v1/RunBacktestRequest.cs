using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class RunBacktestRequest(
   [property: Required(ErrorMessage = "ModelId required")] long ModelId,
   [property: Required(ErrorMessage = "From required")] DateTime From,
   [property: Required(ErrorMessage = "To required")] DateTime To,
   [property: Required(ErrorMessage = "InitialBalance required")] double InitialBalance
);
