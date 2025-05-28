using Mapster;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Bll.Entities;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;
using StageExecutionStatus = TradeWiseBackend.Domain.RepositoryModels.StageExecutionStatus;
using StrategyExecutionInfo = TradeWiseBackend.Domain.RepositoryModels.StrategyExecutionInfo;
using StrategyExecutionStatus = TradeWiseBackend.Domain.RepositoryModels.StrategyExecutionStatus;
using StrategyStage = TradeWiseBackend.Domain.RepositoryModels.StrategyStage;
using StrategyTransition = TradeWiseBackend.Domain.RepositoryModels.StrategyTransition;

namespace TradeWiseBackend.Dal.Repositories;

public class StrategyRepository(DatabaseContext dbContext) : IStrategyRepository
{
    public async Task SaveStrategyStages(List<StrategyStage> strategyStages, CancellationToken ct)
    {
        var strategyStageEntities = strategyStages.Adapt<List<StrategyStageEntity>>();

        await dbContext.StrategyStages.AddRangeAsync(strategyStageEntities);
    }

    public async Task SaveStrategyTransitions(List<StrategyTransition> transitions, CancellationToken ct)
    {
        var entities = transitions.Select(t => new StrategyTransitionEntity
        {
            Id = Guid.NewGuid(),
            StageSourceId = t.StageSourceId,
            StageDestinationId = t.StageDestinationId,
            StrategyId = t.StrategyId,
            StatType = MapStatTypeEntity(t.StatType),
            Operation = MapOperationTypeEntity(t.Operation),
            Value = t.Value,
            InstrumentId = t.InstrumentId
        }).ToList();

        await dbContext.StrategyTransitions.AddRangeAsync(entities);
    }

    public async Task SaveStrategy(Strategy strategy, CancellationToken ct)
    {
        var strategyEntity = strategy.Adapt<StrategyEntity>();

        await dbContext.Strategies.AddAsync(strategyEntity);
    }

    public async Task<List<StrategyInfo>> FetchUserStrategies(string userId, CancellationToken ct)
    {
        return (await dbContext.Strategies
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(ct)).Adapt<List<StrategyInfo>>();
    }

    public async Task<List<StrategyExecutionInfo>> FetchPendingAndRunningStrategies(CancellationToken ct)
    {
        return (await dbContext.StrategyExecutions
            .Where(se =>
                se.Status == Entities.StrategyExecutionStatus.Running ||
                se.Status == Entities.StrategyExecutionStatus.Pending)
            .ToListAsync(ct)).Adapt<List<StrategyExecutionInfo>>();
    }

    public async Task<List<StageExecutionInfo>> FetchPendingStageExecutionsByStrategyExecutionId(
        Guid strategyExecutionId, CancellationToken ct)
    {
        return (await dbContext.StageExecutions
            .Where(n => n.StrategyExecutionId == strategyExecutionId &&
                        n.Status == Entities.StageExecutionStatus.Pending)
            .ToListAsync(ct)).Adapt<List<StageExecutionInfo>>();
    }

    public async Task<List<StrategyTransition>> FetchTransitionByDestinationStage(Guid stageId, CancellationToken ct)
    {
        return (await dbContext.StrategyTransitions
            .Where(t => t.StageDestinationId == stageId)
            .ToListAsync(ct)).Adapt<List<StrategyTransition>>();
    }

    public async Task<StageExecutionInfo> FetchStageExecutionByStageIdAndStrategyExecution(Guid stageId,
        Guid strategyExecutionId, CancellationToken ct)
    {
        return (await dbContext.StageExecutions
                .SingleAsync(se => se.StageId == stageId && se.StrategyExecutionId == strategyExecutionId), ct)
            .Adapt<StageExecutionInfo>();
    }

    public async Task<InfoForExecution> FetchStageWithUserIdByStageExecutionId(Guid stageExecutionId,
        CancellationToken ct)
    {
        var query = await (
            from stage in dbContext.StrategyStages
            join exec in dbContext.StageExecutions
                on stage.Id equals exec.StageId
            join se in dbContext.StrategyExecutions
                on exec.StrategyExecutionId equals se.Id
            where exec.Id == stageExecutionId
            select new InfoForExecution(
                stage.Id,
                stage.StrategyId,
                stage.StageModel,
                stage.Strategy.UserId,
                exec.ExternalExecutionId,
                exec.Id,
                exec.StrategyExecutionId,
                se.IsPaperTrade,
                stage.MaxExecutionDurationSeconds,
                se.AllocatedBudget
            )
        ).SingleOrDefaultAsync(ct);

        return query.Adapt<InfoForExecution>();
    }

    public async Task UpdateStageExecutionStatusByStageExecutionId(Guid stageExecutionId, StageExecutionStatus status,
        CancellationToken ct)
    {
        var convertedStatus = status switch
        {
            StageExecutionStatus.Completed => Entities.StageExecutionStatus.Completed,
            StageExecutionStatus.Failed => Entities.StageExecutionStatus.Failed,
            StageExecutionStatus.Pending => Entities.StageExecutionStatus.Pending,
            StageExecutionStatus.Running => Entities.StageExecutionStatus.Running,
            StageExecutionStatus.Cancelled => Entities.StageExecutionStatus.Cancelled,
            _ => throw new InvalidCastException()
        };

        await dbContext.StageExecutions
            .Where(se => se.Id == stageExecutionId)
            .ExecuteUpdateAsync(se => se
                .SetProperty(x => x.Status, convertedStatus)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task UpdateStrategyExecutionStatusByStrategyExecution(Guid strategyExecutionId,
        StrategyExecutionStatus status, CancellationToken ct)
    {
        var convertedStatus = status switch
        {
            StrategyExecutionStatus.Completed => Entities.StrategyExecutionStatus.Completed,
            StrategyExecutionStatus.Failed => Entities.StrategyExecutionStatus.Failed,
            StrategyExecutionStatus.Pending => Entities.StrategyExecutionStatus.Pending,
            StrategyExecutionStatus.Running => Entities.StrategyExecutionStatus.Running,
            StrategyExecutionStatus.Cancelled => Entities.StrategyExecutionStatus.Cancelled,
            _ => throw new InvalidCastException()
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

    public async Task<List<StageExecutionWithUserId>> FetchRunningStageExecutionsWithUserInfo(CancellationToken ct)
    {
        var query = from se in dbContext.StageExecutions
            join st in dbContext.StrategyStages on se.StageId equals st.Id
            join s in dbContext.Strategies on st.StrategyId equals s.Id
            join u in dbContext.Users on s.UserId equals u.Id
            where se.Status == Entities.StageExecutionStatus.Running
            select new StageExecutionWithUserId(
                se.Id,
                se.StageId,
                MapStageExecutionStatus(se.Status),
                se.ExternalExecutionId,
                s.UserId,
                se.StrategyExecutionId,
                s.Id
            );


        return await query.ToListAsync(ct);
    }

    public async Task<List<StageExecutionInfo>> FetchActiveStageExecutions(Guid strategyExecutionId,
        CancellationToken ct)
    {
        return (await dbContext.StageExecutions
            .Where(se =>
                se.StrategyExecutionId == strategyExecutionId && (se.Status == Entities.StageExecutionStatus.Running ||
                                                                  se.Status == Entities.StageExecutionStatus.Pending))
            .ToListAsync(ct)).Adapt<List<StageExecutionInfo>>();
    }

    public async Task<List<StageInfoCut>> FetchStagesByStrategyId(Guid strategyId, CancellationToken ct)
    {
        return await dbContext.StrategyStages
            .Where(s => s.StrategyId == strategyId)
            .Select(s => new StageInfoCut(
                s.Id,
                s.StageModel,
                s.MaxExecutionDurationSeconds
            ))
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
            UpdatedAt = strategyExecution.UpdatedAt,
            IsPaperTrade = strategyExecution.IsPaperTrade,
            AllocatedBudget = strategyExecution.AllocatedBudget
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
                se.StrategyId,
                se.IsPaperTrade,
                se.AllocatedBudget))
            .ToListAsync(ct);

        return strategyExecutions;
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

    public async Task CancelStageExecutionsBulk(List<Guid> stageIds, Guid strategyExecutionId, CancellationToken ct)
    {
        var executions = await dbContext.StageExecutions
            .Where(se => se.StrategyExecutionId == strategyExecutionId && stageIds.Contains(se.StageId))
            .ToListAsync(ct);

        foreach (var se in executions)
        {
            se.Status = Entities.StageExecutionStatus.Cancelled;
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
            .Where(se =>
                se.StrategyExecutionId == strategyExecutionId && se.Status != Entities.StageExecutionStatus.Cancelled)
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
            .FirstAsync(s => s.Id == strategyId && s.IsActive, ct);

        if (strategy == null) throw new KeyNotFoundException("Strategy not found");

        strategy.IsActive = false;
        strategy.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Guid>> FetchActiveStrategyExecutions(Guid strategyId, CancellationToken ct)
    {
        return await dbContext.StrategyExecutions
            .Where(se => se.StrategyId == strategyId
                         && (se.Status == Entities.StrategyExecutionStatus.Pending ||
                             se.Status == Entities.StrategyExecutionStatus.Running))
            .Select(se => se.Id)
            .ToListAsync(ct);
    }

    public async Task DeleteStrategyStagesByStrategyId(Guid strategyId, CancellationToken ct)
    {
        var stages = dbContext.StrategyStages.Where(s => s.StrategyId == strategyId);

        dbContext.StrategyStages.RemoveRange(stages);

        await Task.CompletedTask;
    }

    public async Task DeleteStrategyTransitionsByStrategyId(Guid strategyId, CancellationToken ct)
    {
        var transitions = dbContext.StrategyTransitions.Where(t => t.StrategyId == strategyId);

        dbContext.StrategyTransitions.RemoveRange(transitions);

        await Task.CompletedTask;
    }

    public async Task UpdateStrategy(Strategy strategy, CancellationToken ct)
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

    public async Task BorrowMoneyFromAllocatedBudget(Guid strategyExecutionId, double borrowedMoney,
        CancellationToken ct)
    {
        var execution = await dbContext.StrategyExecutions
            .SingleAsync(se => se.Id == strategyExecutionId);

        execution.AllocatedBudget -= borrowedMoney;
        execution.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task RefundMoneyIntoAllocatedBudget(Guid strategyExecutionId, double refund, CancellationToken ct)
    {
        var execution = await dbContext.StrategyExecutions
            .SingleAsync(se => se.Id == strategyExecutionId);

        execution.AllocatedBudget += refund;
        execution.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<StrategyExecutionModel>> FetchActiveStrategyExecutionsByUser(string userId,
        CancellationToken ct)
    {
        return await dbContext.StrategyExecutions
            .Where(se => se.Strategy != null && se.Strategy.UserId == userId
                                             && (se.Status == Entities.StrategyExecutionStatus.Pending ||
                                                 se.Status == Entities.StrategyExecutionStatus.Running))
            .Select(se => new StrategyExecutionModel(
                se.Id,
                se.CreatedAt,
                se.UpdatedAt,
                MapStrategyExecutionStatus(se.Status),
                se.StrategyId,
                se.IsPaperTrade,
                se.AllocatedBudget
            ))
            .ToListAsync(ct);
    }

    public async Task<StrategyExecutionModel?> FetchStrategyExecutionById(Guid strategyExecutionId,
        CancellationToken ct)
    {
        return (await dbContext.StrategyExecutions
            .FirstOrDefaultAsync(se => se.Id == strategyExecutionId, ct)).Adapt<StrategyExecutionModel?>();
    }

    private static StatTypeEntity? MapStatTypeEntity(StatType? dtoValue)
    {
        return (StatTypeEntity?)dtoValue;
    }

    private static Entities.StageExecutionStatus MapStageExecutionStatus(StageExecutionStatus status)
    {
        return (Entities.StageExecutionStatus)status;
    }

    private static StageExecutionStatus MapStageExecutionStatus(Entities.StageExecutionStatus status)
    {
        return (StageExecutionStatus)status;
    }

    private static Entities.StrategyExecutionStatus MapStrategyExecutionStatus(StrategyExecutionStatus status)
    {
        return (Entities.StrategyExecutionStatus)status;
    }

    private static StrategyExecutionStatus MapStrategyExecutionStatus(Entities.StrategyExecutionStatus status)
    {
        return (StrategyExecutionStatus)status;
    }

    private static OperationTypeEntity? MapOperationTypeEntity(TransitionConditionType? dtoValue)
    {
        return (OperationTypeEntity?)dtoValue;
    }
}