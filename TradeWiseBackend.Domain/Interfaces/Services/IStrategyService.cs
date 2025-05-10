using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IStrategyService
{
    Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct);
    Task ValidateStrategyStages(ValidateStrategyPayload validateStrategyPayload, CancellationToken ct);
    Task GetUserStrategies(CancellationToken ct);
}