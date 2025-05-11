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
    public DbSet<StrategyEntity> Strategies { get; set; }
    public DbSet<StrategyStageEntity> StrategyStages { get; set; }
    public DbSet<StrategyTransitionEntity> StrategyTransitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StrategyEntity>(entity =>
        {
            entity.ToTable("Strategies");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StrategyStageEntity>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StrategyId });
            entity.Property(e => e.ModelName).IsRequired();
            entity.HasOne(e => e.Strategy)
                .WithMany()
                .HasForeignKey(e => e.StrategyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StrategyTransitionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.StageSource)
                .WithMany()
                .HasForeignKey(e => new { e.StageSourceId, e.StrategyId })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StageDestination)
                .WithMany()
                .HasForeignKey(e => new { e.StageDestinationId, e.StrategyId })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}