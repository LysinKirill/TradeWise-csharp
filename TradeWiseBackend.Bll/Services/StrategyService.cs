using System;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class StrategyService(IStrategyRepository strategyRepository, IAccountRepository accountRepository) : IStrategyService
{
    public async Task CreateStrategyStages(CreateStrategyPayload createStrategyPayload, CancellationToken ct)
    {
        var user = accountRepository.GetUserById(createStrategyPayload.UserId);
        if (user == null)
            throw new Exception("User not found");

        var stages = createStrategyPayload.StrategyStages.Select(stage => new StrategyStage
        (
            Guid.NewGuid(),
            stage.StageModel.ToString(),
            user.GetAwaiter().GetResult()!
        )).ToList();
        
        await strategyRepository.SaveStrategyStages(stages);
    }
}
