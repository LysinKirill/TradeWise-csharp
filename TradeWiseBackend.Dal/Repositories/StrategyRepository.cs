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

    public async Task<List<Domain.RepositoryModels.StrategyExecutionInfo>> GetPendingAndRunningStrategies()
    {
        return (await dbContext.StrategyExecutions
            .Where(se => se.Status == Entities.StrategyExecutionStatus.Running || se.Status == Entities.StrategyExecutionStatus.Pending)
            .ToListAsync()).Adapt<List<Domain.RepositoryModels.StrategyExecutionInfo>>();
    }

    public async Task<List<StageExecutionInfo>> GetPendingStageExecutionsByStrategy(Guid strategyId)
    {
        return (await dbContext.StageExecutions
                .Where(n => n.StrategyExecution != null && n.StrategyExecution.StrategyId == strategyId && (n.Status == Entities.StageExecutionStatus.Pending))
                .ToListAsync()).Adapt<List<StageExecutionInfo>>();
    }

    public async Task<StrategyTransition?> FetchTransitionByDestinationStage(Guid strategyId, Guid stageId)
    {
        return (await dbContext.StrategyTransitions
                .SingleOrDefaultAsync(t => t.StageDestinationId == stageId && t.StrategyId == strategyId)).Adapt<StrategyTransition?>();
    }

    public async Task<StageExecutionInfo> FetchStageExecutionByStageId(Guid stageId, Guid strategyExecutionId)
    {
        return (await dbContext.StageExecutions
            .SingleAsync(se => se.StageId == stageId && se.StrategyExecutionId == strategyExecutionId)).Adapt<StageExecutionInfo>();
    }

    public async Task<StageInfo> FetchStageWithUserByStageId(Guid stageId, Guid stageExecutionId)
    {
        var query = await (
            from stage in dbContext.StrategyStages
            join exec in dbContext.StageExecutions
                on stage.Id equals exec.StageId
            where stage.Id == stageId && exec.Id == stageExecutionId
            select new StageInfo(
                stage.Id,
                stage.StrategyId,
                stage.StageModel,
                stage.Strategy.UserId,
                exec.ExternalExecutionId,
                exec.Id,
                exec.StrategyExecutionId
            )
        ).SingleOrDefaultAsync();


        return query.Adapt<StageInfo>();
    }

    public async Task UpdateStageExecutionStatus(Guid stageExecutionId, Domain.RepositoryModels.StageExecutionStatus status, CancellationToken ct)
    {
        var convertedStatus = status switch
        {
            Domain.RepositoryModels.StageExecutionStatus.Completed => Entities.StageExecutionStatus.Completed,
            Domain.RepositoryModels.StageExecutionStatus.Failed => Entities.StageExecutionStatus.Failed,
            Domain.RepositoryModels.StageExecutionStatus.Pending => Entities.StageExecutionStatus.Pending,
            Domain.RepositoryModels.StageExecutionStatus.Running => Entities.StageExecutionStatus.Running,
            Domain.RepositoryModels.StageExecutionStatus.Cancelled => Entities.StageExecutionStatus.Cancelled,
            _ => throw new NotImplementedException(),
        };

        await dbContext.StageExecutions
            .Where(se => se.Id == stageExecutionId)
            .ExecuteUpdateAsync(se => se
                .SetProperty(x => x.Status, convertedStatus)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

    }

    public async Task UpdateStrategyExecutionStatus(Guid strategyExecutionId, Domain.RepositoryModels.StrategyExecutionStatus status, CancellationToken ct)
    {
        var convertedStatus = status switch
        {
            Domain.RepositoryModels.StrategyExecutionStatus.Completed => Entities.StrategyExecutionStatus.Completed,
            Domain.RepositoryModels.StrategyExecutionStatus.Failed => Entities.StrategyExecutionStatus.Failed,
            Domain.RepositoryModels.StrategyExecutionStatus.Pending => Entities.StrategyExecutionStatus.Pending,
            Domain.RepositoryModels.StrategyExecutionStatus.Running => Entities.StrategyExecutionStatus.Running,
            Domain.RepositoryModels.StrategyExecutionStatus.Cancelled => Entities.StrategyExecutionStatus.Cancelled,
            _ => throw new NotImplementedException(),
        };

        await dbContext.StrategyExecutions
       .Where(ex => ex.Id == strategyExecutionId)
       .Where(ex => ex.Status != convertedStatus)
       .ExecuteUpdateAsync(setters => setters
           .SetProperty(e => e.Status, convertedStatus)
           .SetProperty(e => e.UpdatedAt, DateTime.UtcNow),
           ct);
    }

    public async Task SaveExternalExecutionId(Guid stageExecutionId, long externalExecutionId, CancellationToken ct)
    {
        var stageExecution = await dbContext.StageExecutions
            .Where(se => se.Id == stageExecutionId)
            .SingleAsync(ct);

        stageExecution.ExternalExecutionId = externalExecutionId;
        stageExecution.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<StageExecutionWithUserInfo>> FetchRunningStageExecutionsWithUserInfo(CancellationToken ct)
    {
        var query = from se in dbContext.StageExecutions
                    join st in dbContext.StrategyStages on se.StageId equals st.Id
                    join s in dbContext.Strategies on st.StrategyId equals s.Id
                    join u in dbContext.Users on s.UserId equals u.Id
                    where se.Status == Entities.StageExecutionStatus.Running
                    select new StageExecutionWithUserInfo(
                        se.Id,
                        se.StageId,
                        MapStageExecutionStatus(se.Status),
                        se.ExternalExecutionId,
                        s.UserId,
                        u.Email!,
                        se.StrategyExecutionId,
                        s.Id
                    );


        return await query.ToListAsync(ct);
    }


    public async Task<List<StageExecutionInfo>> FetchActiveStageExecutions(Guid strategyExecutionId, CancellationToken ct)
    {
        return (await dbContext.StageExecutions
            .Where(se => se.StrategyExecutionId == strategyExecutionId && (se.Status == Entities.StageExecutionStatus.Running || se.Status == Entities.StageExecutionStatus.Pending))
            .ToListAsync(ct)).Adapt<List<StageExecutionInfo>>();
    }

    public async Task<Guid> FetchStrategyByStage(Guid stageId, CancellationToken ct)
    {
        return await dbContext.StrategyStages
            .Where(stage => stage.Id == stageId)
            .Select(stage => stage.StrategyId)
            .SingleAsync(ct);
    }

    public async Task<List<Guid>> FetchStagesByStrategyId(Guid strategyId, CancellationToken ct)
    {
        return await dbContext.StrategyStages
            .Where(s => s.StrategyId == strategyId)
            .Select(s => s.Id)
            .ToListAsync(ct);
    }

    public async Task SaveStrategyExecution(StrategyExecutionModel strategyExecution, CancellationToken ct)
    {
        var entity = new StrategyExecutionEntity
        {
            Id = strategyExecution.Id,
            StrategyId = strategyExecution.StrategyId,
            Status = MapStrategyExecutionStatus(strategyExecution.Status),
            CreatedAt = strategyExecution.CreatedAt,
            UpdatedAt = strategyExecution.UpdatedAt
        };

        dbContext.StrategyExecutions.Add(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SaveStageExecutions(List<StageExecutionModel> stageExecutions, CancellationToken ct)
    {
        var entities = stageExecutions.Select(se => new StageExecutionEntity
        {
            Id = se.Id,
            StageId = se.StageId,
            StrategyExecutionId = se.StrategyExecutionId,
            Status = MapStageExecutionStatus(se.Status),
            CreatedAt = se.CreatedAt,
            UpdatedAt = se.UpdatedAt
        }).ToList();

        dbContext.StageExecutions.AddRange(entities);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<StrategyExecutionModel>> FetchStrategyExecutionsByUser(string userId, CancellationToken ct)
    {
        var strategyExecutions = await dbContext.StrategyExecutions
            .Where(se => se.Strategy!.UserId == userId)
            .Select(se => new StrategyExecutionModel(
                se.Id,
                se.CreatedAt,
                se.UpdatedAt,
                MapStrategyExecutionStatus(se.Status),
                se.StrategyId))
            .ToListAsync(ct);

        return strategyExecutions;
    }

    public async Task<List<StrategyExecutionModel>> FetchStrategyExecutionsByStrategyId(Guid strategyId, CancellationToken ct)
    {
        var executions = await dbContext.StrategyExecutions
            .Where(se => se.StrategyId == strategyId)
            .Select(se => new StrategyExecutionModel(
                se.Id,
                se.CreatedAt,
                se.UpdatedAt,
                MapStrategyExecutionStatus(se.Status),
                se.StrategyId))
            .ToListAsync(ct);

        return executions;
    }

    public async Task<List<StrategyTransition>> FetchTransitionByStrategyId(Guid strategyId, CancellationToken ct)
    {
        return (await dbContext.StrategyTransitions
            .Where(t => t.StrategyId == strategyId)
            .ToListAsync(ct)).Adapt<List<StrategyTransition>>();
    }

    public async Task FailStageExecutionsBulk(List<Guid> stageIds, Guid strategyExecutionId, CancellationToken ct)
    {
        var executions = await dbContext.StageExecutions
            .Where(se => se.StrategyExecutionId == strategyExecutionId && stageIds.Contains(se.StageId))
            .ToListAsync(ct);

        foreach (var se in executions)
        {
            se.Status = Entities.StageExecutionStatus.Failed;
            se.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task CancelActiveStagesAndStrategyExecution(Guid strategyExecutionId, CancellationToken ct)
    {
        await dbContext.StrategyExecutions
            .Where(se => se.Id == strategyExecutionId && se.Status != Entities.StrategyExecutionStatus.Cancelled)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(se => se.Status, Entities.StrategyExecutionStatus.Cancelled)
                .SetProperty(se => se.UpdatedAt, DateTime.UtcNow),
                ct);

        await dbContext.StageExecutions
            .Where(se => se.StrategyExecutionId == strategyExecutionId && se.Status != Entities.StageExecutionStatus.Cancelled)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(se => se.Status, Entities.StageExecutionStatus.Cancelled)
                .SetProperty(se => se.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public async Task<List<long>> FetchExternalExecutionId(Guid strategyExecutionId, CancellationToken ct)
    {
        return await dbContext.StageExecutions
        .Where(se => se.StrategyExecutionId == strategyExecutionId && se.ExternalExecutionId != null)
        .Select(se => se.ExternalExecutionId!.Value)
        .ToListAsync(ct);
    }
    public async Task DeleteStrategy(Guid strategyId, CancellationToken ct)
    {
        var strategy = await dbContext.Strategies
            .SingleAsync(s => s.Id == strategyId && s.IsActive, ct);

        strategy.IsActive = false;
        strategy.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
    }
    public async Task<List<Guid>> FetchActiveStrategyExecutions(Guid strategyId, CancellationToken ct)
    {
        return await dbContext.StrategyExecutions
            .Where(se => se.StrategyId == strategyId
                && (se.Status == Entities.StrategyExecutionStatus.Pending || se.Status == Entities.StrategyExecutionStatus.Running))
            .Select(se => se.Id)
            .ToListAsync(ct); ;
    }

    public async Task DeleteStrategyStagesByStrategyId(Guid strategyId)
    {
        var stages = dbContext.StrategyStages.Where(s => s.StrategyId == strategyId);

        dbContext.StrategyStages.RemoveRange(stages);

        await Task.CompletedTask;
    }

    public async Task DeleteStrategyTransitionsByStrategyId(Guid strategyId)
    {
        var transitions = dbContext.StrategyTransitions.Where(t => t.StrategyId == strategyId);

        dbContext.StrategyTransitions.RemoveRange(transitions);

        await Task.CompletedTask;
    }

    public async Task UpdateStrategy(Strategy strategy)
    {
        var convertedEntity = new StrategyEntity
        {
            Id = strategy.Id,
            Title = strategy.Title,
            Description = strategy.Description,
            UserId = strategy.UserId,
            CreatedAt = strategy.CreatedAt,
            UpdatedAt = strategy.UpdatedAt,
            IsActive = true
        };
        dbContext.Strategies.Update(convertedEntity);

        await Task.CompletedTask;
    }

    public async Task<Strategy> FetchStrategyById(Guid strategyId, CancellationToken ct)
    {
        return (await dbContext.Strategies
            .AsNoTracking()
            .SingleAsync(s => s.Id == strategyId, ct)).Adapt<Strategy>();
    }

    private static StatTypeEntity MapStatTypeEntity(StatType dtoValue)
    {
        return (StatTypeEntity)dtoValue;
    }

    private static Entities.StageExecutionStatus MapStageExecutionStatus(Domain.RepositoryModels.StageExecutionStatus status)
    {
        return (Entities.StageExecutionStatus)status;
    }
    private static Domain.RepositoryModels.StageExecutionStatus MapStageExecutionStatus(Entities.StageExecutionStatus status)
    {
        return (Domain.RepositoryModels.StageExecutionStatus)status;
    }

    private static Entities.StrategyExecutionStatus MapStrategyExecutionStatus(Domain.RepositoryModels.StrategyExecutionStatus status)
    {
        return (Entities.StrategyExecutionStatus)status;
    }
    private static Domain.RepositoryModels.StrategyExecutionStatus MapStrategyExecutionStatus(Entities.StrategyExecutionStatus status)
    {
        return (Domain.RepositoryModels.StrategyExecutionStatus)status;
    }

    private static OperationTypeEntity MapOperationTypeEntity(TransitionConditionType dtoValue)
    {
        return (OperationTypeEntity)dtoValue;
    }
}