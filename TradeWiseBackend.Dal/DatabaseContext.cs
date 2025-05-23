using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public DbSet<StrategyExecutionEntity> StrategyExecutions { get; set; }
    public DbSet<StageExecutionEntity> StageExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stageExecutionStatusConverter = new ValueConverter<StageExecutionStatus, string>(
            v => v.ToString(),
            v => (StageExecutionStatus)Enum.Parse(typeof(StageExecutionStatus), v));

        var strategyExecutionStatusConverter = new ValueConverter<StrategyExecutionStatus, string>(
            v => v.ToString(),
            v => (StrategyExecutionStatus)Enum.Parse(typeof(StrategyExecutionStatus), v));

        modelBuilder.Entity<StrategyExecutionEntity>()
            .Property(e => e.Status)
            .HasConversion(strategyExecutionStatusConverter)
            .HasMaxLength(50);

        modelBuilder.Entity<StageExecutionEntity>()
            .Property(e => e.Status)
            .HasConversion(stageExecutionStatusConverter)
            .HasMaxLength(50);

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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StageModel).IsRequired();
            entity.HasOne(e => e.Strategy)
                .WithMany()
                .HasForeignKey(e => e.StrategyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StrategyTransitionEntity>(entity =>
        {
            entity.Property(e => e.StatType)
                .HasConversion<string>();

            entity.Property(e => e.Operation)
                .HasConversion<string>();

            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.StageSource)
                .WithMany()
                .HasForeignKey(e => e.StageSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StageDestination)
                .WithMany()
                .HasForeignKey(e => e.StageDestinationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StrategyExecutionEntity>(entity =>
        {
            entity.ToTable("StrategyExecutions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(e => e.Strategy)
                .WithMany()
                .HasForeignKey(e => e.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StageExecutionEntity>(entity =>
        {
            entity.ToTable("StageExecutions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(e => e.Stage)
                .WithMany()
                .HasForeignKey(e => e.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.StrategyExecution)
                .WithMany()
                .HasForeignKey(e => e.StrategyExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}