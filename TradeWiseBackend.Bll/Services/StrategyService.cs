using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Model;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class StrategyService(IStrategyRepository strategyRepository,
    IAccountRepository accountRepository,
    StrategyValidationService validator,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    ModelService.ModelServiceClient modelServiceClient)
    : IStrategyService
{
    private Metadata AuthMetadata
    {
        get
        {
            var token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (token is null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "No authorization header provided"));
            return new Metadata
            {
                { "Authorization", token }
            };
        }
    }

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
        var stageExecutionEntities = strategyStages.Select(stage => new StageExecutionModel
        (
            Guid.NewGuid(),
            stage.Id,
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

    public async Task CancelStrategy(CancelStrategyPayload cancelStrategyPayload, CancellationToken ct)
    {
        var externalExecutionIds = await strategyRepository.FetchExternalExecutionId(cancelStrategyPayload.StrategyExecutionId, ct);
        foreach (var execution in externalExecutionIds)
        {
            var request = new StopExecutionRequest
            {
                ExecutionId = execution
            };

            await modelServiceClient.StopExecutionAsync(request, headers: AuthMetadata, cancellationToken: ct);
        }

        await strategyRepository.CancelActiveStagesAndStrategyExecution(cancelStrategyPayload.StrategyExecutionId, ct);
    }

    public async Task DeleteStrategy(DeleteStrategyPayload deleteStrategyPayload, CancellationToken ct)
    {
        var activeStrategyExecutions = await strategyRepository.FetchActiveStrategyExecutions(deleteStrategyPayload.StrategyId, ct);
        if (activeStrategyExecutions.Count != 0)
        {
            throw new InvalidOperationException($"The strategy cannot be deleted: there are active executions ({activeStrategyExecutions.Count}).");
        }

        await strategyRepository.DeleteStrategy(deleteStrategyPayload.StrategyId, ct);
    }

    public async Task EditStrategy(EditStrategyPayload editStrategyPayload, CancellationToken ct)
    {
        var existingStrategy = await strategyRepository.FetchStrategyById(editStrategyPayload.StrategyId, ct);
        if (existingStrategy == null)
        {
            throw new Exception("Strategy not found");
        }

        var (isValid, error) = validator.Validate(editStrategyPayload.StrategyStages, editStrategyPayload.StrategyTransitions);
        if (!isValid)
        {
            throw new ValidationException(error!);
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var updatedStrategy = existingStrategy with
            {
                Title = editStrategyPayload.Title,
                Description = editStrategyPayload.Description,
                UpdatedAt = DateTime.UtcNow
            };

            await strategyRepository.UpdateStrategy(updatedStrategy);

            await strategyRepository.DeleteStrategyStagesByStrategyId(editStrategyPayload.StrategyId);
            await strategyRepository.DeleteStrategyTransitionsByStrategyId(editStrategyPayload.StrategyId);

            var stageIdMap = new Dictionary<Guid, Guid>();
            foreach (var stage in editStrategyPayload.StrategyStages)
            {
                stageIdMap[stage.Id] = Guid.NewGuid();
            }

            var newStages = editStrategyPayload.StrategyStages.Select(stage => new StrategyStage(
                stageIdMap[stage.Id],
                editStrategyPayload.StrategyId,
                stage.ModelId
            )).ToList();
            await strategyRepository.SaveStrategyStages(newStages);

            var newTransitions = new List<StrategyTransition>();
            foreach (var transition in editStrategyPayload.StrategyTransitions)
            {
                if (transition.SourceStageId == null || transition.DestinationStageId == null)
                    continue;

                foreach (var condition in transition.TransitionConditions)
                {
                    var entity = new StrategyTransition(
                        Guid.NewGuid(),
                        stageIdMap[transition.SourceStageId.Value],
                        stageIdMap[transition.DestinationStageId.Value],
                        editStrategyPayload.StrategyId,
                        condition.StatType,
                        condition.TransitionConditionType,
                        condition.Value
                    );
                    newTransitions.Add(entity);
                }
            }
            await strategyRepository.SaveStrategyTransitions(newTransitions);

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<FullStrategyInfo> GetStrategy(GetStrategyPayload payload, CancellationToken ct)
    {
        var strategy = await strategyRepository.FetchStrategyById(payload.StrategyId, ct);
        var stages = await strategyRepository.FetchStagesByStrategyId(payload.StrategyId, ct);
        var transitions = await strategyRepository.FetchTransitionByStrategyId(payload.StrategyId, ct);

        var groupedTransitions = transitions
            .GroupBy(t => new { t.StageSourceId, t.StageDestinationId })
            .Select(g => new Domain.Models.StrategyTransition(
                SourceStageId: g.Key.StageSourceId,
                DestinationStageId: g.Key.StageDestinationId,
                TransitionConditions: g.Select(t => new Domain.Models.TransitionCondition(
                    (Domain.Models.TransitionConditionType)t.StatType,
                    (Domain.Models.StatType)t.Operation,
                    t.Value
                )).ToList()
            ))
            .ToList();

        if (strategy == null)
            throw new Exception("Strategy not found");

        var convertedStages = stages.Select(se => new Domain.Models.StrategyStage
        (
            se.Id,
            se.ModelId
        )).ToList();

        var allDestinationStageIds = groupedTransitions.Select(t => t.DestinationStageId).ToHashSet();
        var rootStage = stages.Where(s => !allDestinationStageIds.Contains(s.Id)).ToList().Single();

        var allSourceStageIds = transitions.Select(t => t.StageSourceId).ToHashSet();
        var leafStages = stages.Where(s => !allSourceStageIds.Contains(s.Id)).ToList();

        groupedTransitions.Add(new Domain.Models.StrategyTransition
        (
            null,
            rootStage.Id,
            []
        ));

        foreach (var leaf in leafStages)
        {
            groupedTransitions.Add(new Domain.Models.StrategyTransition
            (
                leaf.Id,
                null,
                []
            ));
        }

        var strategyInfo = new FullStrategyInfo
        (
            strategy.Id,
            strategy.Title,
            strategy.Description,
            strategy.CreatedAt,
            strategy.UpdatedAt,
            convertedStages,
            groupedTransitions
        );

        return strategyInfo;
    }
}