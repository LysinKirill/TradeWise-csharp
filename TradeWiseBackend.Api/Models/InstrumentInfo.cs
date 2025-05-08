namespace TradeWiseBackend.Api.Models;

public record InstrumentInfo
{
    public required string Id { get; init; }
    public required string Figi { get; init; }
    public required string Name { get; init; }
    public required int Lot { get; init; }
    public required string Currency { get; init; }
    public required string Sector { get; init; }
    public required bool BuyAvailable { get; init; }
    public required bool SellAvailable { get; init; }
}
