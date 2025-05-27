using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IAccountService
{
    Task<AccountOverviewInfo> GetAccountOverview(string userId, CancellationToken ct);
    Task<List<StrategyExecutionInfo>> GetUserExecutions(string userId, CancellationToken ct);
}