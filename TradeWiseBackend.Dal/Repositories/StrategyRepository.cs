using System;
using Mapster;
using TradeWiseBackend.Bll.Entities;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.RepositoryModels;
using Microsoft.EntityFrameworkCore;

namespace TradeWiseBackend.Dal.Repositories;

public class StrategyRepository(DatabaseContext dbContext) : IStrategyRepository
{
    public async Task SaveStrategyStages(List<Domain.RepositoryModels.StrategyStage> strategyStages)
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

    public async Task SaveStrategyTransitions(List<Domain.RepositoryModels.StrategyTransition> transitions)
    {
        var entities = transitions.Select(t => new StrategyTransitionEntity
        {
            StrategyTransitionId = Guid.NewGuid(),
            StageSourceId = t.StageSourceId,
            StageDestinationId = t.StageDestinationId,
            StatType = MapStatTypeEntity(t.StatType),
            Operation = MapOperationTypeEntity(t.Operation),
            Value = t.Value
        }).ToList();

        await dbContext.StrategyTransitions.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    private static StatTypeEntity MapStatTypeEntity(StatType dtoValue)
    {
        return (StatTypeEntity)dtoValue;
    }

    private static OperationTypeEntity MapOperationTypeEntity(TransitionConditionType dtoValue)
    {
        return (OperationTypeEntity)dtoValue;
    }
}
