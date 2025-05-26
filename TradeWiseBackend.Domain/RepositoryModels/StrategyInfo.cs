namespace TradeWiseBackend.Domain.ServiceModels;

public record class StrategyInfo(
    Guid Id,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsPaperTrade,
    double AllocatedBudget
);
