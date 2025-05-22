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
        var query = await (
            from stage in dbContext.StrategyStages
            join execution in dbContext.StageExecutions
                on stage.Id equals execution.StageId into executionsGroup
            from exec in executionsGroup.DefaultIfEmpty()
            where stage.Id == stageId
            select new StageInfo(
                stage.Id,
                stage.StrategyId,
                stage.StageModel,
                stage.Strategy.UserId,
                exec != null ? exec.ExternalExecutionId : null
            )
        ).SingleOrDefaultAsync();

        return query.Adapt<StageInfo>();
    }

    public async Task UpdateStageExecutionStatus(Guid stageId, Guid strategyId, Domain.RepositoryModels.StageExecutionStatus status, CancellationToken ct)
    {
        var convertedStatus = status switch
        {
            Domain.RepositoryModels.StageExecutionStatus.Completed => Entities.StageExecutionStatus.Completed,
            Domain.RepositoryModels.StageExecutionStatus.Failed => Entities.StageExecutionStatus.Failed,
            Domain.RepositoryModels.StageExecutionStatus.Pending => Entities.StageExecutionStatus.Pending,
            Domain.RepositoryModels.StageExecutionStatus.Running => Entities.StageExecutionStatus.Running,
            _ => throw new NotImplementedException(),
        };

        await dbContext.StageExecutions
            .Where(se => se.StageId == stageId &&
                        se.StrategyExecution.StrategyId == strategyId)
            .ExecuteUpdateAsync(se => se
                .SetProperty(x => x.Status, convertedStatus)
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

    public async Task SaveExternalExecutionId(Guid stageId, long externalExecutionId, CancellationToken ct)
    {
        var stageExecution = await dbContext.StageExecutions
            .Where(se => se.StageId == stageId)
            .SingleAsync(ct);

        if (stageExecution == null)
        {
            throw new KeyNotFoundException($"No StageExecution found with StageId {stageId}");
        }

        stageExecution.ExternalExecutionId = externalExecutionId;
        stageExecution.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
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