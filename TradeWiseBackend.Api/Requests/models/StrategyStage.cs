using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.models;

public record StrategyStage(
    [param: Required(ErrorMessage = "Id required")] Guid? Id,
    [param: Required(ErrorMessage = "ModelId required")] long? ModelId,
    [param: Required(ErrorMessage = "MaxExecutionDurationSeconds required")] int? MaxExecutionDurationSeconds
);