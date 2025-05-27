using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class EditStrategyPayload(
    Guid StrategyId,
    string Title,
    string? Description,
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions,
    string UserId
);