using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Authorize]
[Route(RoutesV1.StrategyApi)]
public class StrategyController(IStrategyService strategyService) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStrategy(CreateStrategyRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var createPayload = request.Adapt<CreateStrategyPayload>() with { UserId = userId };
        await strategyService.CreateStrategyStages(
            createPayload, ct);

        return Ok();
    }

    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateStrategy(ValidateStrategyRequest request,
        CancellationToken ct)
    {
        var validatePayload = request.Adapt<ValidateStrategyPayload>();
        await strategyService.ValidateStrategyStages(
            validatePayload, ct);

        return Ok();
    }
}