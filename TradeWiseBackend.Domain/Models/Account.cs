namespace TradeWiseBackend.Domain.Models;

public record Account
{
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
};