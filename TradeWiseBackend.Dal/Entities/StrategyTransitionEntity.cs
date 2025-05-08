using System;
using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Bll.Entities;

public class StrategyTransitionEntity
{
    [Key] public Guid StrategyTransitionId { get; set; }

    public Guid? StageSourceId { get; set; }
    public Guid? StageDestinationId { get; set; }
    public StatTypeEntity StatType { get; set; }
    public OperationTypeEntity Operation { get; set; }
    public double Value { get; set; }
}