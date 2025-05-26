namespace TradeWiseBackend.Api.Requests.v1;

public record class RunBacktestRequest(
   long ModelId,
   DateTime From,
   DateTime To,
   double InitialBalance
);
