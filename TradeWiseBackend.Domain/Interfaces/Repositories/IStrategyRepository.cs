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
    Task<List<StageExecutionInfo>> GetPendingAndRunningStageExecutionsByStrategy(Guid strategyId);
    Task<StrategyTransition?> FetchTransitionByDestinationStage(Guid strategyId, Guid stageId);
    Task<StageExecutionInfo> FetchStageExecutionById(Guid stageIds);
}