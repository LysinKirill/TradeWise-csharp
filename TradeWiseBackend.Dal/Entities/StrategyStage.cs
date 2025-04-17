using System;
using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Bll.Entities;

public class StrategyStage
{
    [Key] public Guid StageId { get; set; }

    public required string ModelName { get; set; }
    public required string UserId { get; set; }
    public required AccountEntity User { get; set; }
}