using System.Runtime.InteropServices;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IStrategyRepository
{
    Task SaveStrategyStages(List<StrategyStage> strategy);
}