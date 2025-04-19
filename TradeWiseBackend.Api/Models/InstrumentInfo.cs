namespace TradeWiseBackend.Api.Models;

public record class InstrumentInfo
{
    public required string id { get; init; }
    public required string figi { get; init; }
    public required string name { get; init; }
    public required int lot { get; init; }
    public required string currency { get; init; }
    public required string sector { get; init; }
    public required bool buy_available { get; init; }
    public required bool sell_available { get; init; }
}
