using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
    Task SaveStrategyTransitions(List<StrategyTransition> transitions);
    Task<List<StrategyInfo>> FetchUserStrategies(string userId);
    Task SaveStrategy(Strategy strategy);
    Task<List<RepositoryModels.StrategyExecutionInfo>> GetPendingAndRunningStrategies();
    Task<List<StageExecutionInfo>> GetPendingStageExecutionsByStrategy(Guid strategyId);
    Task<StrategyTransition?> FetchTransitionByDestinationStage(Guid strategyId, Guid stageId);
    Task<StageExecutionInfo> FetchStageExecutionByStageId(Guid stageId, Guid strategyExecutionId);
    Task<StageInfo> FetchStageWithUserByStageId(Guid stageId, Guid stageExecution);
    Task UpdateStageExecutionStatus(Guid stageId, StageExecutionStatus status, CancellationToken ct);
    Task UpdateStrategyExecutionStatus(Guid strategyExecutionId, StrategyExecutionStatus status, CancellationToken ct);
    Task SaveExternalExecutionId(Guid stageExecutionId, long externalExecutionId, CancellationToken ct);
    Task<List<StageExecutionWithUserInfo>> FetchRunningStageExecutionsWithUserInfo(CancellationToken ct);
    Task<List<StageExecutionInfo>> FetchActiveStageExecutions(Guid strategyExecutionId, CancellationToken ct);
    Task SaveStageExecutions(List<StageExecutionModel> stageExecution, CancellationToken ct);
    Task SaveStrategyExecution(StrategyExecutionModel strategyExecution, CancellationToken ct);
    Task<Guid> FetchStrategyByStage(Guid stageId, CancellationToken ct);
    Task<List<Guid>> FetchStagesByStrategyId(Guid strategyId, CancellationToken ct);
    Task<List<StrategyExecutionModel>> FetchStrategyExecutionsByUser(string userId, CancellationToken ct);
    Task<List<StrategyExecutionModel>> FetchStrategyExecutionsByStrategyId(Guid strategyId, CancellationToken ct);
    Task<List<StrategyTransition>> FetchTransitionByStrategyId(Guid strategyId, CancellationToken ct);
    Task FailStageExecutionsBulk(List<Guid> stageIds, Guid strategyExecutionId, CancellationToken ct);
    Task CancelActiveStagesAndStrategyExecution(Guid strategyExecutionId, CancellationToken ct);
    Task<List<long>> FetchExternalExecutionId(Guid strategyExecutionId, CancellationToken ct);
}