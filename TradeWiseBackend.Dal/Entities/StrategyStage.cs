using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyStage
{
    [Key] public Guid StageId { get; set; } // UUID stage_id

    public required string ModelName { get; set; } // string model_name
    public Guid StageSourceId { get; set; } // UUID stage_source_id
    public Guid StageDestinationId { get; set; } // UUID stage_destination_id
}