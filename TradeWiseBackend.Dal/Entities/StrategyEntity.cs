using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

[Table("Strategies")]
public class StrategyEntity
{
    [Key]
    public Guid StrategyId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string UserId { get; set; }
    public AccountEntity? User { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}
