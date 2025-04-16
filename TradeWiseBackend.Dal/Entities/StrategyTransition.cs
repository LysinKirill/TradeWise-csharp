using System;
using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Bll.Entities;

public class StrategyTransition
{
    [Key] public Guid StrategyTransitionId { get; set; }

    public Guid? StageSourceId { get; set; }
    public Guid? StageDestinationId { get; set; }
    public StatType StatType { get; set; }
    public OperationType Operation { get; set; }
    public double Value { get; set; }
}