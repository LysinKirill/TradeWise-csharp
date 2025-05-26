using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyExecutionEntity
{
    [Key]
    public Guid Id { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public required StrategyExecutionStatus Status { get; set; }

    [ForeignKey(nameof(Strategy))]
    public Guid StrategyId { get; set; }
    public StrategyEntity? Strategy { get; set; }

    public bool IsPaperTrade { get; set; }
    public double UsedBudget { get; set; }
}
