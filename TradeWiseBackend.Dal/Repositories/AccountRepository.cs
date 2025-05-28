using Mapster;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Dal.Repositories;

public class AccountRepository(DatabaseContext dbContext) : IAccountRepository
{
    public async Task<Account?> GetUserById(string userId)
    {
        var user = await dbContext.Accounts.SingleAsync(u => u.Id == userId);

        return user?.Adapt<Account>();
    }

    public async Task AddAsync(RefreshTokenModel model)
    {
        var entity = model.Adapt<RefreshTokenEntity>();
        await dbContext.RefreshTokens.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<RefreshTokenModel?> GetByTokenAsync(string token)
    {
        return (await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token)).Adapt<RefreshTokenModel?>();
    }

    public async Task UpdateAsync(RefreshTokenModel model)
    {
        var entity = await dbContext.RefreshTokens.FindAsync(model.Id);

        if (entity == null) throw new InvalidOperationException("Token not found");

        model.Adapt(entity);

        await dbContext.SaveChangesAsync();
    }
}