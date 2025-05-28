using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeWiseBackend.Dal.Entities;

public class RefreshTokenEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public string Token { get; set; } = null!;

    [Required] public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))] public AccountEntity User { get; set; } = null!;

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required] public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;
}