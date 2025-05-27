using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Api.Responses.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;
using BacktestInfo = TradeWiseBackend.Api.Responses.models.BacktestInfo;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Authorize]
[Route(RoutesV1.BacktestApi)]
public class BacktestController(IBacktestService backtestService) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunBacktest(RunBacktestRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var payload = request.Adapt<RunBacktestPayload>() with { UserId = userId };
        await backtestService.RunBacktest(payload, ct);
        return Ok();
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelBacktest(CancelBacktestRequest request, CancellationToken ct)
    {
        var payload = request.Adapt<CancelBacktestPayload>();
        await backtestService.CancelBacktest(payload, ct);
        return Ok();
    }

    [HttpPost("all-backtests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<GetAllBacktestsResponse> GetAllBacktestsBacktest(CancellationToken ct)
    {
        var backtests = await backtestService.GetAllBacktests(ct);
        return new GetAllBacktestsResponse(backtests.Adapt<List<BacktestInfo>>());
    }
}