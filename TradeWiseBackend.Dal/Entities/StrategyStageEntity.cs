using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyStageEntity
{
    public Guid StrategyId { get; set; }
    public Guid StageId { get; set; }

    public required string ModelName { get; set; }
    public required string UserId { get; set; }
    public AccountEntity? User { get; set; }
}