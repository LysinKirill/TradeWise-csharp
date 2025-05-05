using System;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class StrategyService : IStrategyService
{
    public Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
