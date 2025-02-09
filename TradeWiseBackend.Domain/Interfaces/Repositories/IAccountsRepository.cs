using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IAccountsRepository
{
    Task Create(Account account);
}