using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Bll.Entities;

public class StrategyTransitionEntity
{
    [Key] public Guid Id { get; set; }
    public Guid StageSourceId { get; set; }
    public Guid StageDestinationId { get; set; }
    public Guid StrategyId { get; set; }

    [ForeignKey(nameof(StageSourceId))] public StrategyStageEntity? StageSource { get; set; }

    [ForeignKey(nameof(StageDestinationId))]
    public StrategyStageEntity? StageDestination { get; set; }

    public StatTypeEntity StatType { get; set; }
    public OperationTypeEntity Operation { get; set; }
    public double Value { get; set; }
    public required string InstrumentId { get; set; }
}