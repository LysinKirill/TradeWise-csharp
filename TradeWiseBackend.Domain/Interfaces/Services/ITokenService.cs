using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface ITokenService
{
    Task<string> GenerateToken(AccountEntityModel user);
}