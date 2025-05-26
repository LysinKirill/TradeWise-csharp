using System;
using Backtest;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class BacktestService(Backtest.BacktestService.BacktestServiceClient backtestClient, IBacktestRepository backtestRepository, IHttpContextAccessor httpContextAccessor) : IBacktestService
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

    public async Task RunBacktest(RunBacktestPayload payload, CancellationToken ct)
    {
        var request = new StartBacktestRequest
        {
            ModelId = payload.ModelId,
            From = Timestamp.FromDateTime(payload.From),
            To = Timestamp.FromDateTime(payload.To),
            InitialBalance = payload.InitialBalance
        };
        var response = await backtestClient.StartBacktestAsync(request, headers: AuthMetadata, cancellationToken: ct);

        var entityToSave = new BacktestExecution(
            Guid.NewGuid(),
            response.BacktestId,
            payload.UserId
        );
        await backtestRepository.Save(entityToSave, ct);
    }

    public async Task CancelBacktest(CancelBacktestPayload payload, CancellationToken ct)
    {
        var externalExecutionId = await backtestRepository.GetExternalExecutionId(payload.BacktestExecutionId, ct);

        var request = new CancelBacktestRequest
        {
            BacktestId = externalExecutionId
        };
        await backtestClient.CancelBacktestAsync(request, headers: AuthMetadata, cancellationToken: ct);
    }

    public async Task<List<Domain.ServiceModels.BacktestInfo>> GetAllBacktests(string userId, CancellationToken ct)
    {
        var backtests = await backtestClient.GetAllUserBacktestsAsync(new Empty(), headers: AuthMetadata, cancellationToken: ct);
        return backtests.Adapt<List<Domain.ServiceModels.BacktestInfo>>();
    }
}
