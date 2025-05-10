using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IStrategyService
{
    Task CreateStrategyStages(CreateStrategyPayload createStrategyPayload, CancellationToken ct);
    Task ValidateStrategyStages(ValidateStrategyPayload validateStrategyPayload, CancellationToken ct);
    Task GetUserStrategies(CancellationToken ct);
}