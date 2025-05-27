using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record GetInstrumentStatRequest
{
    [property: Required(ErrorMessage = "InstrumentId required")]
    public required string InstrumentId { get; init; }

    [property: Required(ErrorMessage = "StatType required")]
    public required StatType StatType { get; init; }

    public DateTime From { get; init; } = DateTime.Now.AddMinutes(-5);
    public DateTime To { get; init; } = DateTime.Now;
}