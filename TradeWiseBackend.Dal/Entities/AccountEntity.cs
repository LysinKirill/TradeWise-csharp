using Microsoft.AspNetCore.Identity;

namespace TradeWiseBackend.Dal.Entities;

public class AccountEntity : IdentityUser
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}