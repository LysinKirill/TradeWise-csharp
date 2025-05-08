using System;
using Mapster;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Dal.Repositories;

public class StrategyRepository(DatabaseContext dbContext) : IStrategyRepository
{
    public async Task SaveStrategyStages(List<StrategyStage> strategyStages)
    {
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();
        foreach (var stageEntity in strategyStageEntities)
        {
            var originalStage = strategyStages.First(s => s.StageId == stageEntity.StageId);

            stageEntity.UserId = originalStage.User.Id;
            stageEntity.User = null;
        }

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveStrategyTransitions(List<StrategyStage> strategyStages)
    {
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();
        foreach (var stageEntity in strategyStageEntities)
        {
            var originalStage = strategyStages.First(s => s.StageId == stageEntity.StageId);

            stageEntity.UserId = originalStage.User.Id;
            stageEntity.User = null;
        }

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
        await dbContext.SaveChangesAsync();
    }
}
