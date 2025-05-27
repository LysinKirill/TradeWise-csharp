using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    // ---------------------------------- read-write --------------------------------------

    Task SaveStrategy(Strategy strategy, CancellationToken ct);
    Task SaveStrategyStages(List<RepositoryModels.StrategyStage> strategy, CancellationToken ct);
    Task SaveStrategyTransitions(List<RepositoryModels.StrategyTransition> transitions, CancellationToken ct);

    Task SaveStageExecutions(List<StageExecutionModel> stageExecution, CancellationToken ct);
    Task SaveStrategyExecution(StrategyExecutionModel strategyExecution, CancellationToken ct);
    Task SaveExternalExecutionId(Guid stageExecutionId, long externalExecutionId, CancellationToken ct);

    Task FailStageExecutionsBulk(List<Guid> stageIds, Guid strategyExecutionId, CancellationToken ct);
    Task CancelStageExecutionsBulk(List<Guid> stageIds, Guid strategyExecutionId, CancellationToken ct);
    Task CancelActiveStagesAndStrategyExecution(Guid strategyExecutionId, CancellationToken ct);

    Task UpdateStrategy(Strategy strategy, CancellationToken ct);
    Task UpdateStageExecutionStatusByStageExecutionId(Guid stageExecutionId, StageExecutionStatus status, CancellationToken ct);
    Task UpdateStrategyExecutionStatusByStrategyExecution(Guid strategyExecutionId, RepositoryModels.StrategyExecutionStatus status, CancellationToken ct);

    Task DeleteStrategy(Guid strategyId, CancellationToken ct);
    Task DeleteStrategyStagesByStrategyId(Guid strategyId, CancellationToken ct);

    Task BorrowMoneyFromAllocatedBudget(Guid strategyExecutionId, double borrowedMoney, CancellationToken ct);
    Task RefundMoneyIntoAllocatedBudget(Guid strategyExecutionId, double refund, CancellationToken ct);

    // ---------------------------------- read-only ---------------------------------------

    Task<List<StrategyInfo>> FetchUserStrategies(string userId, CancellationToken ct);
    Task<List<StrategyExecutionModel>> FetchStrategyExecutionsByUser(string userId, CancellationToken ct);
    Task<List<StrategyExecutionModel>> FetchActiveStrategyExecutionsByUser(string userId, CancellationToken ct);

    Task<List<RepositoryModels.StrategyExecutionInfo>> FetchPendingAndRunningStrategies(CancellationToken ct);
    Task<List<StageExecutionInfo>> FetchPendingStageExecutionsByStrategyExecutionId(Guid strategyExecutionId, CancellationToken ct);
    Task<List<RepositoryModels.StrategyTransition>> FetchTransitionByDestinationStage(Guid stageId, CancellationToken ct);
    Task<StageExecutionInfo> FetchStageExecutionByStageIdAndStrategyExecution(Guid stageId, Guid strategyExecutionId, CancellationToken ct);
    Task<InfoForExecution> FetchStageWithUserIdByStageExecutionId(Guid stageExecutionId, CancellationToken ct);
    Task<List<StageExecutionWithUserId>> FetchRunningStageExecutionsWithUserInfo(CancellationToken ct);
    Task<List<StageExecutionInfo>> FetchActiveStageExecutions(Guid strategyExecutionId, CancellationToken ct);
    Task<List<StageInfoCut>> FetchStagesByStrategyId(Guid strategyId, CancellationToken ct);
    Task<List<RepositoryModels.StrategyTransition>> FetchTransitionByStrategyId(Guid strategyId, CancellationToken ct);
    Task<List<long>> FetchExternalExecutionId(Guid strategyExecutionId, CancellationToken ct);
    Task<List<Guid>> FetchActiveStrategyExecutions(Guid strategyId, CancellationToken ct);
    Task DeleteStrategyTransitionsByStrategyId(Guid strategyId, CancellationToken ct);
    Task<Strategy> FetchStrategyById(Guid strategyId, CancellationToken ct);
    Task<StrategyExecutionModel?> FetchStrategyExecutionById(Guid strategyExecutionId, CancellationToken ct);
}