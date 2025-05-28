using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Responses.models;
using TradeWiseBackend.Api.Responses.v1;
using TradeWiseBackend.Domain.Interfaces.Services;

namespace TradeWiseBackend.Api.Controllers;

[Authorize]
[Route(RoutesV1.AccountApi)]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpGet("get-overview")]
    [ProducesResponseType<GetAccountOverviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSupportedInstruments(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var accountOverview = await accountService.GetAccountOverview(userId, ct);

        return Ok(accountOverview.Adapt<GetAccountOverviewResponse>());
    }

    [HttpGet("executions")]
    [ProducesResponseType<GetAccountOverviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserExecutions(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var executions = await accountService.GetUserExecutions(userId, ct);
        var converted_executions = executions.Select(info => new StrategyExecution(
            info.Id,
            info.CreatedAt,
            info.UpdatedAt,
            MapStatus(info.Status),
            info.StrategyId)).ToList();

        return Ok(new GetUserExecutionsResponse(converted_executions));
    }

    private StrategyExecutionStatus MapStatus(Domain.Models.StrategyExecutionStatus? status)
    {
        return status switch
        {
            Domain.Models.StrategyExecutionStatus.Cancelled => StrategyExecutionStatus.Cancelled,
            Domain.Models.StrategyExecutionStatus.Completed => StrategyExecutionStatus.Completed,
            Domain.Models.StrategyExecutionStatus.Failed => StrategyExecutionStatus.Failed,
            Domain.Models.StrategyExecutionStatus.Pending => StrategyExecutionStatus.Pending,
            Domain.Models.StrategyExecutionStatus.Running => StrategyExecutionStatus.Running,
            _ => throw new InvalidCastException($"Unknown BacktestStatus {status}")
        };
    }
}