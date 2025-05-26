using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IStrategyService
{
    Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct);
    Task ValidateStrategyStages(ValidateStrategyPayload validateStrategyPayload, CancellationToken ct);
    Task<List<StrategyGeneralInfo>> GetUserStrategies(string userId, CancellationToken ct);
    Task RunStrategy(RunStrategyPayload runStrategyPayload, string userId, CancellationToken ct);
    Task CancelStrategy(CancelStrategyPayload cancelStrategyPayload, CancellationToken ct);
    Task DeleteStrategy(DeleteStrategyPayload deleteStrategyPayload, CancellationToken ct);
    Task EditStrategy(EditStrategyPayload editStrategyPayload, CancellationToken ct);
    Task<FullStrategyInfo> GetStrategy(GetStrategyPayload payload, CancellationToken ct);
}