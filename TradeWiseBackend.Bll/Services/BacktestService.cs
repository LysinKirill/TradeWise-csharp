using Backtest;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.RepositoryModels;
using TradeWiseBackend.Domain.ServiceModels;
using BacktestInfo = TradeWiseBackend.Domain.ServiceModels.BacktestInfo;
using BacktestStatus = TradeWiseBackend.Domain.ServiceModels.BacktestStatus;

namespace TradeWiseBackend.Bll.Services;

public class BacktestService(
    Backtest.BacktestService.BacktestServiceClient backtestClient,
    IBacktestRepository backtestRepository,
    IHttpContextAccessor httpContextAccessor) : IBacktestService
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
            From = Timestamp.FromDateTime(payload.From.ToUniversalTime()),
            To = Timestamp.FromDateTime(payload.To.ToUniversalTime()),
            InitialBalance = payload.InitialBalance
        };
        var response = await backtestClient.StartBacktestAsync(request, AuthMetadata, cancellationToken: ct);

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
        await backtestClient.CancelBacktestAsync(request, AuthMetadata, cancellationToken: ct);
    }

    public async Task<List<BacktestInfo>> GetAllBacktests(CancellationToken ct)
    {
        var backtests = await backtestClient.GetAllUserBacktestsAsync(new Empty(), AuthMetadata, cancellationToken: ct);
        return backtests.Backtests.Select(b => new BacktestInfo(
            b.BacktestId,
            b.StartedAt?.ToDateTime(),
            b.FinishedAt?.ToDateTime(),
            b.TestPeriodStart?.ToDateTime(),
            b.TestPeriodEnd?.ToDateTime(),
            MapBacktestStatus(b.Status),
            b.Profit,
            b.TradesCount,
            b.InitialBalance,
            b.FinalBalance,
            b.CreatedAt.ToDateTime()
        )).ToList();
    }

    private BacktestStatus? MapBacktestStatus(Backtest.BacktestStatus? status)
    {
        if (status == null) return null;
        return status switch
        {
            Backtest.BacktestStatus.Cancelled => BacktestStatus.Cancelled,
            Backtest.BacktestStatus.Completed => BacktestStatus.Completed,
            Backtest.BacktestStatus.Failed => BacktestStatus.Failed,
            Backtest.BacktestStatus.Pending => BacktestStatus.Pending,
            Backtest.BacktestStatus.Running => BacktestStatus.Running,
            _ => throw new InvalidCastException($"Unknown BacktestStatus {status}")
        };
    }
}