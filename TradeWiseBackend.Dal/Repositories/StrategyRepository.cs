using System;
using Mapster;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Dal.Repositories;

public class StrategyRepository(DatabaseContext dbContext) : IStrategyRepository
{
    public async Task SaveStrategyStages(List<Domain.Models.StrategyStage> strategyStages)
    {
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
        await dbContext.SaveChangesAsync();
    }
}
