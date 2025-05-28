using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TradeWiseBackend.Api.Configuration;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.RepositoryModels;

namespace TradeWiseBackend.Bll.Services;

public class TokenService : ITokenService
{
    private readonly IAccountRepository _accountRepository;
    private readonly JwtSettings _jwtSettings;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IAccountRepository accountRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _accountRepository = accountRepository;
    }

    public string GenerateToken(AccountEntityModel user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: creds,
            claims: claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
    {
        var entity = new RefreshTokenModel
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _accountRepository.AddAsync(entity);
    }

    public async Task<(bool IsValid, string? UserId)> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _accountRepository.GetByTokenAsync(refreshToken);

        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
            return (false, null);

        return (true, tokenEntity.UserId);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _accountRepository.GetByTokenAsync(refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            await _accountRepository.UpdateAsync(tokenEntity);
        }
    }
}