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
        Console.WriteLine("KEKE1 ");
        foreach (var stage in strategyStages)
        {
            Console.WriteLine(stage.User.Id);
        }
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();
        foreach (var stageEntity in strategyStageEntities)
        {
            var originalStage = strategyStages.First(s => s.StageId == stageEntity.StageId);

            stageEntity.UserId = originalStage.User.Id;
        }
        Console.WriteLine("KEKE2 ");
         foreach (var stage in strategyStageEntities)
        {
            Console.WriteLine(stage.UserId);
            Console.WriteLine(stage.User.Id);
        }

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
        await dbContext.SaveChangesAsync();
    }
}
