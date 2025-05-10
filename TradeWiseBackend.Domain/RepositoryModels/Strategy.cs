namespace TradeWiseBackend.Domain.RepositoryModels;

public record class Strategy(
    Guid StrategyId,
    string Title,
    string? Description,
    string UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
