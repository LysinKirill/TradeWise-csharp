using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class RunStrategyPayload(
    Guid StrategyId
);
