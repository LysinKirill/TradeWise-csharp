using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
    Task SaveStrategyTransitions(List<StrategyTransition> transitions);
}