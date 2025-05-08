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
    public DbSet<StrategyStageEntity> StrategyStages { get; set; }
    public DbSet<StrategyTransitionEntity> StrategyTransitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StrategyStageEntity>(entity =>
        {
            entity.HasKey(e => new { e.StageId, e.StrategyId });
            entity.Property(e => e.ModelName).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<StrategyTransitionEntity>(entity =>
        {
            entity.HasKey(e => e.StrategyTransitionId);

            entity.HasOne(e => e.StageSource)
                .WithMany()
                .HasForeignKey("StageSourceId", "StrategyId")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StageDestination)
                .WithMany()
                .HasForeignKey("StageDestinationId", "StrategyId")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}