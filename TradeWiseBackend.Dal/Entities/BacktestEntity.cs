using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

public class BacktestExecutionEntity
{
    [Key] public Guid Id { get; set; }

    public long ExternalExecutionId { get; set; }

    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))] public AccountEntity? User { get; set; }

    public DateTime CreatedAt { get; set; }
}