using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class ValidateStrategyPayload(
    List<StrategyStage> StrategyStages,
    List<StrategyTransition> StrategyTransitions
);
