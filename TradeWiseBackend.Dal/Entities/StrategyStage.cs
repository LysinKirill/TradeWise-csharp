using System;
using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Bll.Entities;

public class StrategyStage
{
    [Key] public Guid StageId { get; set; }

    public required string ModelName { get; set; }
}