using Mapster;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Dal.Repositories;

public class AccountsRepository(DatabaseContext databaseContext) : IAccountsRepository
{
    public async Task Create(Account account)
    {
        await databaseContext.Accounts.AddAsync(account.Adapt<AccountEntity>());
        await databaseContext.SaveChangesAsync();
    }
}