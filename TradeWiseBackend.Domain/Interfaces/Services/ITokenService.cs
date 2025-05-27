using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(AccountEntityModel user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(string userId, string refreshToken);
    Task<(bool IsValid, string? UserId)> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}