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
    Task UpdateStageExecutionStatus(Guid stageId, Guid strategyId, StageExecutionStatus status, CancellationToken ct);
    Task UpdateStrategyExecutionStatusToRunning(Guid stageId, Guid strategyId, CancellationToken ct);
    Task SaveExternalExecutionId(Guid stageId, long externalExecutionId, CancellationToken ct);
}