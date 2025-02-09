using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Dal;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : IdentityDbContext<AccountEntity>(options)
{
    public required DbSet<AccountEntity> Accounts { get; set; }
}