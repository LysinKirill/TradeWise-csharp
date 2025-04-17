using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeWiseBackend.Bll.Entities;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Dal;

public class DatabaseContext : IdentityDbContext<AccountEntity>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public required DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<StrategyStage> StrategyStages { get; set; }
    public DbSet<StrategyTransition> StrategyTransitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StrategyStage>(entity =>
        {
            entity.HasKey(e => e.StageId);
            entity.Property(e => e.ModelName).IsRequired();

            entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired();
        });

        modelBuilder.Entity<StrategyTransition>(entity =>
        {
            entity.HasKey(e => e.StrategyTransitionId);
        });
    }
}