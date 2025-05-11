using Mapster;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Bll.Entities;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;
using Microsoft.AspNetCore.Mvc;

using StrategyStage = TradeWiseBackend.Domain.RepositoryModels.StrategyStage;
using StrategyTransition = TradeWiseBackend.Domain.RepositoryModels.StrategyTransition;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Dal.Repositories;

public class StrategyRepository(DatabaseContext dbContext) : IStrategyRepository
{
    public async Task SaveStrategyStages(List<StrategyStage> strategyStages)
    {
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveStrategyTransitions(List<StrategyTransition> transitions)
    {
        var entities = transitions.Select(t => new StrategyTransitionEntity
        {
            Id = Guid.NewGuid(),
            StageSourceId = t.StageSourceId,
            StageDestinationId = t.StageDestinationId,
            StrategyId = t.StrategyId,
            StatType = MapStatTypeEntity(t.StatType),
            Operation = MapOperationTypeEntity(t.Operation),
            Value = t.Value
        }).ToList();

        foreach (var t in entities)
        {
            Console.WriteLine("KEKE! " + t.StageDestinationId);
            Console.WriteLine("KEKE!! " + t.StageSourceId);
            Console.WriteLine("KEKEGTGT " + t.StrategyId);
        }

        await dbContext.StrategyTransitions.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveStrategy(Strategy strategy)
    {
        var strategyEntity = strategy.Adapt<StrategyEntity>();

        await dbContext.Strategies.AddAsync(strategyEntity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<StrategyInfo>> FetchUserStrategies(string userId)
    {
        return (await dbContext.Strategies
            .Where(s => s.UserId == userId)
            .ToListAsync()).Adapt<List<StrategyInfo>>();
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