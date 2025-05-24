using Mapster;
using Microsoft.EntityFrameworkCore;
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
}