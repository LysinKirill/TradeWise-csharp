using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class StrategyService(IStrategyRepository strategyRepository, IAccountRepository accountRepository)
    : IStrategyService
{
    public async Task CreateStrategyStages(CreateStrategyPayload createStrategyPayload, CancellationToken ct)
    {
        var user = accountRepository.GetUserById(createStrategyPayload.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var stageIdMap = new Dictionary<Guid, Guid>();
        foreach (var stage in createStrategyPayload.StrategyStages) stageIdMap[stage.Id] = Guid.NewGuid();

        var strategyId = Guid.NewGuid();

        var stages = createStrategyPayload.StrategyStages.Select(stage => new StrategyStage
        (
            stageIdMap[stage.Id],
            strategyId,
            stage.StageModel,
            user.GetAwaiter().GetResult()!
        )).ToList();

        var transitionEntities = new List<StrategyTransition>();

        // TODO: валидировать что нет и первой и последней пустых нод
        // TODO: валидировать что такой ноды или перехода уже нет
        foreach (var transition in createStrategyPayload.StrategyTransitions)
            foreach (var condition in transition.TransitionConditions)
            {
                var entity = new StrategyTransition
                (
                    Guid.NewGuid(),
                    transition.SourceStageId.HasValue ? stageIdMap[transition.SourceStageId.Value] : null,
                    transition.DestinationStageId.HasValue ? stageIdMap[transition.DestinationStageId.Value] : null,
                    strategyId,
                    condition.StatType,
                    condition.TransitionConditionType,
                    condition.Value
                );

                transitionEntities.Add(entity);
            }

        await strategyRepository.SaveStrategyStages(stages);
        await strategyRepository.SaveStrategyTransitions(transitionEntities);
    }
}