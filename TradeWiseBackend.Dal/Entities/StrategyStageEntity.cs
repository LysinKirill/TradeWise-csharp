using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyStageEntity
{
    public Guid StrategyId { get; set; }
    public required StrategyEntity Strategy { get; set; }
    public Guid StageId { get; set; }

    public required string ModelName { get; set; }
}