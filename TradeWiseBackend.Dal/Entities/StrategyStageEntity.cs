using System;
using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Dal.Entities;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyStageEntity
{
    [Key] public Guid StageId { get; set; }

    public required string ModelName { get; set; }
    public required string UserId { get; set; }
    public AccountEntity? User { get; set; }
}