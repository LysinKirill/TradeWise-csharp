using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyExecutionEntity
{
    [Key]
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required string Status { get; set; }

    [ForeignKey(nameof(Strategy))]
    public Guid StrategyId { get; set; }
    public required StrategyEntity Strategy { get; set; }
}
