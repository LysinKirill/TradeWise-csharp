using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.v1;

public record class CancelBacktestRequest(
    [property: Required(ErrorMessage = "BacktestExecutionId required")] Guid BacktestExecutionId
);
