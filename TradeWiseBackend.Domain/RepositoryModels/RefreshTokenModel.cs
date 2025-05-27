namespace TradeWiseBackend.Domain.RepositoryModels;

public class RefreshTokenModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public Account User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
}