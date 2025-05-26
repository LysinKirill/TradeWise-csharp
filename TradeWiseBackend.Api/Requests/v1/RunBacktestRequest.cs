using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class RunBacktestRequest(
   [param: Required(ErrorMessage = "ModelId required")] long? ModelId,
   [param: Required(ErrorMessage = "From required")] DateTime? From,
   [param: Required(ErrorMessage = "To required")] DateTime? To,
   [param: Required(ErrorMessage = "InitialBalance required")] double? InitialBalance
);
