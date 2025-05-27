using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class FullStrategyInfo(
    Guid Id,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);