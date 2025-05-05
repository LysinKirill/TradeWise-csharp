using System;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IStrategyService
{
    Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct);
}
