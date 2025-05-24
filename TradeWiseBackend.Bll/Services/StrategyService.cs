using System.ComponentModel.DataAnnotations;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class StrategyService(IStrategyRepository strategyRepository, IAccountRepository accountRepository, StrategyValidationService validator, IUnitOfWork unitOfWork)
    : IStrategyService
{
    public async Task CreateStrategy(CreateStrategyPayload createStrategyPayload, CancellationToken ct)
    {
        // TODO: декомпозировать
        var user = await accountRepository.GetUserById(createStrategyPayload.UserId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var (isValid, error) = validator.Validate(createStrategyPayload.StrategyStages, createStrategyPayload.StrategyTransitions);

        if (!isValid)
        {
            throw new ValidationException(error!);
        }

        var stageIdMap = new Dictionary<Guid, Guid>();
        foreach (var stage in createStrategyPayload.StrategyStages) stageIdMap[stage.Id] = Guid.NewGuid();

        var strategyId = Guid.NewGuid();

        var stages = createStrategyPayload.StrategyStages.Select(stage => new StrategyStage
        (
            stageIdMap[stage.Id],
            strategyId,
            stage.ModelId
        )).ToList();

        var transitionEntities = new List<StrategyTransition>();

        foreach (var transition in createStrategyPayload.StrategyTransitions)
        {
            if (transition.SourceStageId == null || transition.DestinationStageId == null)
            {
                continue;
            }

            foreach (var condition in transition.TransitionConditions)
            {
                var entity = new StrategyTransition
                (
                    Guid.NewGuid(),
                    stageIdMap[transition.SourceStageId.Value],
                    stageIdMap[transition.DestinationStageId.Value],
                    strategyId,
                    condition.StatType,
                    condition.TransitionConditionType,
                    condition.Value
                );

                transitionEntities.Add(entity);
            }
        }

        var strategy = new Strategy(
            strategyId,
            createStrategyPayload.Title,
            createStrategyPayload.Description,
            createStrategyPayload.UserId,
            DateTime.Now.ToUniversalTime(),
            DateTime.Now.ToUniversalTime()
        );

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await strategyRepository.SaveStrategy(strategy);
            await strategyRepository.SaveStrategyStages(stages);
            await strategyRepository.SaveStrategyTransitions(transitionEntities);

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<StrategyGeneralInfo>> GetUserStrategies(string userId, CancellationToken ct)
    {
        var strategies = await strategyRepository.FetchUserStrategies(userId);

        return strategies.Adapt<List<StrategyGeneralInfo>>();
    }

    public Task ValidateStrategyStages(ValidateStrategyPayload validateStrategyPayload, CancellationToken ct)
    {
        var (isValid, error) = validator.PreValidate(validateStrategyPayload.StrategyStages, validateStrategyPayload.StrategyTransitions);

        if (!isValid)
        {
            throw new ValidationException(error!);
        }

        return Task.CompletedTask;
    }

    public async Task RunStrategy(RunStrategyPayload runStrategyPayload, CancellationToken ct)
    {
        var strategyStages = await strategyRepository.FetchStagesByStrategyId(runStrategyPayload.StrategyId, ct);
        var strategyExecution = new StrategyExecutionModel(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            StrategyExecutionStatus.Pending,
            runStrategyPayload.StrategyId
        );
        var stageExecutionEntities = strategyStages.Select(stageId => new StageExecutionModel
        (
            Guid.NewGuid(),
            stageId,
            strategyExecution.Id,
            StageExecutionStatus.Pending,
            DateTime.UtcNow,
            DateTime.UtcNow
        )).ToList();

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await strategyRepository.SaveStrategyExecution(strategyExecution, ct);
            await strategyRepository.SaveStageExecutions(stageExecutionEntities, ct);

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}