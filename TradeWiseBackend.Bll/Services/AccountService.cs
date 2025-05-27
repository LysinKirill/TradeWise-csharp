using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;
using User;

namespace TradeWiseBackend.Bll.Services;

public class AccountService(
    IStrategyRepository strategyRepository,
    UserService.UserServiceClient userServiceClient,
    IHttpContextAccessor httpContextAccessor) : IAccountService
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

    public async Task<AccountOverviewInfo> GetAccountOverview(string userId, CancellationToken ct)
    {
        var strategiesCount = (await strategyRepository.FetchUserStrategies(userId, ct)).Count;

        var potfolioInfo = await userServiceClient.GetPortfolioAsync(new Empty(), AuthMetadata, cancellationToken: ct);

        double totalPnl = potfolioInfo.Positions.Sum(position => position.DailyYield);

        return new AccountOverviewInfo(
            strategiesCount,
            potfolioInfo.RubleBalance,
            potfolioInfo.Positions.Adapt<List<PortfolioPosition>>(),
            totalPnl
        );
    }

    public async Task<List<StrategyExecutionInfo>> GetUserExecutions(string userId, CancellationToken ct)
    {
        var userExecutions = await strategyRepository.FetchStrategyExecutionsByUser(userId, ct);

        var result = userExecutions.Select(s => new StrategyExecutionInfo(
            s.Id,
            s.CreatedAt,
            s.UpdatedAt,
            ConvertStatus(s.Status),
            s.StrategyId)).ToList();

        return result;
    }

    private static StrategyExecutionStatus ConvertStatus(Domain.RepositoryModels.StrategyExecutionStatus status)
    {
        return status switch
        {
            Domain.RepositoryModels.StrategyExecutionStatus.Pending => StrategyExecutionStatus.Pending,
            Domain.RepositoryModels.StrategyExecutionStatus.Running => StrategyExecutionStatus.Running,
            Domain.RepositoryModels.StrategyExecutionStatus.Completed => StrategyExecutionStatus.Completed,
            Domain.RepositoryModels.StrategyExecutionStatus.Cancelled => StrategyExecutionStatus.Cancelled,
            Domain.RepositoryModels.StrategyExecutionStatus.Failed => StrategyExecutionStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}