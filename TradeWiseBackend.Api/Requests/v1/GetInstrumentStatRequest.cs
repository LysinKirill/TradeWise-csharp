using TradeWiseBackend.Api.Requests.models;

namespace TradeWiseBackend.Api.Requests.v1;

public record GetInstrumentStatRequest
{
    public required string InstrumentId { get; init; }
    public required StatType StatType { get; init; }
    public DateTime From { get; init; } = DateTime.Now;
    public DateTime To { get; init; } = DateTime.Now.AddMinutes(-5);
};
