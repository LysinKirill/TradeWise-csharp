using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Requests.models;

public record StrategyStage(
    [property: Required(ErrorMessage = "Id required")] Guid Id,
    [property: Required(ErrorMessage = "ModelId required")] long ModelId
);