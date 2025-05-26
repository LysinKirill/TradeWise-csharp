using System;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IBacktestService
{
    Task RunBacktest(RunBacktestPayload payload, CancellationToken ct);
    Task CancelBacktest(CancelBacktestPayload payload, CancellationToken ct);
    Task<List<BacktestInfo>> GetAllBacktests(CancellationToken ct);
}
