using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Dal.DatabaseSettings;

public class DbContext(DbContextOptions<DbContext> options): IdentityDbContext<AccountEntity>(options)
{
    public required DbSet<IdentityUser> Accounts { get; set; }
}