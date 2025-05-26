namespace TradeWiseBackend.Domain.RepositoryModels;

public record class Strategy(
    Guid Id,
    string Title,
    string? Description,
    string UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double AllocatedBudget
);
