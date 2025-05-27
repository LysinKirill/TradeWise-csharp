using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetUserById(string userId);
    Task AddAsync(RefreshTokenModel entity);
    Task<RefreshTokenModel?> GetByTokenAsync(string token);
    Task UpdateAsync(RefreshTokenModel entity);
}