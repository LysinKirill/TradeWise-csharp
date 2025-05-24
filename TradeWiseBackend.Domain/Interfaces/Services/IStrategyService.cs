using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IStrategyService
{
    Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct);
    Task ValidateStrategyStages(ValidateStrategyPayload validateStrategyPayload, CancellationToken ct);
    Task<List<StrategyGeneralInfo>> GetUserStrategies(string userId, CancellationToken ct);
    Task RunStrategy(RunStrategyPayload runStrategyPayload, CancellationToken ct);
}