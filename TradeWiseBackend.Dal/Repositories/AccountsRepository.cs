using Mapster;
using TradeWiseBackend.Dal.DatabaseSettings;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Dal.Repositories;

public class AccountsRepository(DbContext dbContext): IAccountsRepository
{
    public async Task Create(Account account)
    {
        await dbContext.Accounts.AddAsync(account.Adapt<AccountEntity>());
        await dbContext.SaveChangesAsync();
    }
}