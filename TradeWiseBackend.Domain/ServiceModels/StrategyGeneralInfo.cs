namespace TradeWiseBackend.Domain.ServiceModels;

public record class StrategyGeneralInfo(
    Guid Id,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double Profit
);
