using Mapster;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Bll.Entities;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;

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

    public async Task<List<StrategyExecutionInfo>> GetPendingAndRunningStrategies()
    {
        return (await dbContext.StrategyExecutions
        .Where(se => se.Status == StrategyExecutionStatus.Running || se.Status == StrategyExecutionStatus.Pending)
        .ToListAsync()).Adapt<List<StrategyExecutionInfo>>();
    }

    public async Task<List<StageExecutionInfo>> GetPendingStageExecutionsByStrategy(Guid strategyId)
    {
        return (await dbContext.StageExecutions
                .Where(n => n.StrategyExecution.Id == strategyId && (n.Status == Entities.StageExecutionStatus.Pending))
                .ToListAsync()).Adapt<List<StageExecutionInfo>>();
    }

    public async Task<StrategyTransition?> FetchTransitionByDestinationStage(Guid strategyId, Guid stageId)
    {
        return (await dbContext.StrategyTransitions
                .SingleAsync(t => t.StageDestinationId == stageId && t.StrategyId == strategyId)).Adapt<StrategyTransition?>();
    }

    public async Task<StageExecutionInfo> FetchStageExecutionByStageId(Guid stageId)
    {
        return (await dbContext.StageExecutions
            .SingleAsync(se => se.StageId == stageId)).Adapt<StageExecutionInfo>();
    }

    public async Task<StageInfo> FetchStageWithUserByStageId(Guid stageId)
    {
        var query = await dbContext.StrategyStages
        .Include(ss => ss.Strategy)
        .Where(ss => ss.Id == stageId)
        .Select(ss => new StageInfo(
            ss.Id,
            ss.StrategyId,
            ss.StageModel,
            ss.Strategy.UserId
        )).SingleAsync();

        return query.Adapt<StageInfo>();
    }

    public async Task UpdateStageExecutionStatusToRunning(Guid stageId, Guid strategyId, CancellationToken ct)
    {
        await dbContext.StageExecutions
            .Where(se => se.StageId == stageId &&
                        se.StrategyExecution.StrategyId == strategyId)
            .ExecuteUpdateAsync(se => se
                .SetProperty(x => x.Status, Entities.StageExecutionStatus.Running)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

    }

    public async Task UpdateStrategyExecutionStatusToRunning(Guid stageId, Guid strategyId, CancellationToken ct)
    {
        await dbContext.StrategyExecutions
            .Where(ex => dbContext.StageExecutions
                .Any(se => se.ExecutionId == ex.Id
                        && se.StageId == stageId))
            .Where(ex => ex.StrategyId == strategyId && ex.Status != StrategyExecutionStatus.Running)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, StrategyExecutionStatus.Running)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow), ct);

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