using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

public class StageExecutionEntity
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Stage))]
    public Guid StageId { get; set; }
    public required StrategyStageEntity Stage { get; set; }

    [ForeignKey(nameof(StrategyExecution))]
    public Guid ExecutionId { get; set; }
    public required StrategyExecutionEntity StrategyExecution { get; set; }

    public required string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
