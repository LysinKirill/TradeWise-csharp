using System;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IAccountService
{
    Task<AccountOverviewInfo> GetAccountOverview(string userId, CancellationToken ct);
}
