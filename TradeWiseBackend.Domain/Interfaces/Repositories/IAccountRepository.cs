using System;
using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetUserById(string userId);
}
