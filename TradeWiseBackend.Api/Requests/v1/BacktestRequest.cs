namespace TradeWiseBackend.Api.Requests.v1;

public record class BacktestRequest(
   Guid StrategyId,
   long ModelId,
   DateTime From,
   DateTime To,
   double InitialBalance
);
