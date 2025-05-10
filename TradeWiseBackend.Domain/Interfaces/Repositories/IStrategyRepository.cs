using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
    Task SaveStrategyTransitions(List<StrategyTransition> transitions);
    Task<IActionResult> FetchUserStrategies(string userId);
    Task SaveStrategy(Strategy strategy);
}