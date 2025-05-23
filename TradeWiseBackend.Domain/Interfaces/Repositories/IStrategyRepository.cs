using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
    Task SaveStrategyTransitions(List<StrategyTransition> transitions);
    Task<List<StrategyInfo>> FetchUserStrategies(string userId);
    Task SaveStrategy(Strategy strategy);
    Task<List<StrategyExecutionInfo>> GetPendingAndRunningStrategies();
    Task<List<StageExecutionInfo>> GetPendingStageExecutionsByStrategy(Guid strategyId);
    Task<StrategyTransition?> FetchTransitionByDestinationStage(Guid strategyId, Guid stageId);
    Task<StageExecutionInfo> FetchStageExecutionByStageId(Guid stageIds);
    Task<StageInfo> FetchStageWithUserByStageId(Guid stageId);
    Task UpdateStageExecutionStatus(Guid stageId, StageExecutionStatus status, CancellationToken ct);
    Task UpdateStrategyExecutionStatusByStrategyId(Guid stageId, StrategyExecutionStatus status, CancellationToken ct);
    Task SaveExternalExecutionId(Guid stageId, long externalExecutionId, CancellationToken ct);
    Task<List<StageExecutionInfo>> FetchRunningStageExecutions(CancellationToken ct);
    Task<List<StageExecutionInfo>> FetchActiveStageExecutionsByStrategy(Guid strategyId, CancellationToken ct);
    public Task SaveStageExecution(StageExecutionModel stageExecution, CancellationToken ct);
    public Task SaveStrategyExecution(StrategyExecutionModel strategyExecution, CancellationToken ct);
    Task<Guid> FetchStrategyByStage(Guid stageId, CancellationToken ct);
}