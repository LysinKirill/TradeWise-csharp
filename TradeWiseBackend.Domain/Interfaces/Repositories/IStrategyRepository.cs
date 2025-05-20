using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
    Task SaveStrategyTransitions(List<StrategyTransition> transitions);
    Task<List<StrategyInfo>> FetchUserStrategies(string userId);
    Task SaveStrategy(Strategy strategy);
    Task<List<StrategyExecutionInfo>> GetActiveStrategies();
    Task<List<StageExecutionInfo>> GetStagesByStrategy(Guid strategyId);
}