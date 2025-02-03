using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.Interfaces.Interfaces.Repositories;

public interface IAccountsRepository
{
    Task Create(Account account);
}