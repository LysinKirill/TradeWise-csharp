using Microsoft.AspNetCore.Identity;

namespace TradeWiseBackend.Dal.Entities;

public class AccountEntity : IdentityUser
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}