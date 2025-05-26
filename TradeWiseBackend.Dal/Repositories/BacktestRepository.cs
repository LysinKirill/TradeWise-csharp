using System;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Dal.Repositories;

public class BacktestRepository(DatabaseContext dbContext) : IBacktestRepository
{
    public async Task Save(BacktestExecution execution, CancellationToken ct)
    {
        var convertedEntity = new BacktestExecutionEntity
        {
            Id = execution.Id,
            ExternalExecutionId = execution.ExternalExecutionId,
            CreatedAt = DateTime.Now.ToUniversalTime(),
            UserId = execution.UserId
        };
        await dbContext.BacktestExecutions.AddAsync(convertedEntity, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<long> GetExternalExecutionId(Guid executionId, CancellationToken ct)
    {
        return await dbContext.BacktestExecutions
            .Where(se => se.Id == executionId)
            .Select(se => se.ExternalExecutionId)
            .SingleAsync(ct);
    }
}
