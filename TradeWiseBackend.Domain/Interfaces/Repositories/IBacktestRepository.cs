using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IBacktestRepository
{
    Task Save(BacktestExecution execution, CancellationToken ct);
    Task<long> GetExternalExecutionId(Guid executionId, CancellationToken ct);
}