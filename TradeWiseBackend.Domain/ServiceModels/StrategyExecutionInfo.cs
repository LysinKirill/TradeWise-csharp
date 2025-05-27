using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record StrategyExecutionInfo(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    StrategyExecutionStatus Status,
    Guid StrategyId
);