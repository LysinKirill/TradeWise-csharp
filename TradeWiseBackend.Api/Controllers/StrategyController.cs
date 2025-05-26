using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.models;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Api.Responses.v1;
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

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        CreateStrategyPayload createPayload;
        try
        {
            createPayload = MapCreateRequest(request, userId, ct);
        }
        catch (InvalidCastException ex)
        {
            return BadRequest(ex.Message);
        }
        
        await strategyService.CreateStrategy(
            createPayload, ct);

        return Ok();
    }

    [HttpPost("validate")]
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

    [HttpGet("user-strategies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserStrategies(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userStrategies = await strategyService.GetUserStrategies(userId, ct);

        return Ok(userStrategies);
    }

    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunStrategy(RunStrategyRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var runStrategyPayload = request.Adapt<RunStrategyPayload>();
        await strategyService.RunStrategy(runStrategyPayload, userId, ct);

        return Ok();
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelStrategy(CancelStrategyRequest request, CancellationToken ct)
    {
        var cancelStrategyPayload = request.Adapt<CancelStrategyPayload>();
        await strategyService.CancelStrategy(cancelStrategyPayload, ct);

        return Ok();
    }

    [HttpPost("edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EditStrategy(EditStrategyRequest request, CancellationToken ct)
    {
        var editStrategyPayload = request.Adapt<EditStrategyPayload>();
        await strategyService.EditStrategy(editStrategyPayload, ct);
        return Ok();
    }

    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteStrategy(DeleteStrategyRequest request, CancellationToken ct)
    {
        var deleteStrategyPayload = request.Adapt<DeleteStrategyPayload>();
        try
        {
            await strategyService.DeleteStrategy(deleteStrategyPayload, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        return Ok();
    }

    [HttpGet("get")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<GetStrategyResponse> GetStrategy([FromQuery] GetStrategyRequest request, CancellationToken ct)
    {
        var getStrategyPayload = request.Adapt<GetStrategyPayload>();
        var strategy = await strategyService.GetStrategy(getStrategyPayload, ct);
        return strategy.Adapt<GetStrategyResponse>();
    }

    private CreateStrategyPayload MapCreateRequest(CreateStrategyRequest request, string userId,
        CancellationToken ct)
    {
        var convertedStages = request.StrategyStages.Select(s => new Domain.Models.StrategyStage(
                s.Id!.Value,
                s.ModelId!.Value,
                s.MaxExecutionDurationSeconds!.Value
            )).ToList();
        
        var convertedTransitions = request.StrategyTransitions.Select(s => new Domain.Models.StrategyTransition(
                s.SourceStageId,
                s.DestinationStageId,
                s.TransitionConditions.Select(t => new Domain.Models.TransitionCondition(
                    t.TransitionConditionType switch
                    {
                        TransitionConditionType.EqualTo => Domain.Models.TransitionConditionType.EqualTo,
                        TransitionConditionType.GreaterThan => Domain.Models.TransitionConditionType.GreaterThan,
                        TransitionConditionType.LessThan => Domain.Models.TransitionConditionType.LessThan,
                        _ => throw new InvalidCastException($"Unknown operation type {t.TransitionConditionType}")
                    },
                    t.StatType switch
                    {
                        StatType.BollingerBandLower => Domain.Models.StatType.BollingerBandLower,
                        StatType.BollingerBandMiddle => Domain.Models.StatType.BollingerBandMiddle,
                        StatType.BollingerBandUpper => Domain.Models.StatType.BollingerBandUpper,
                        StatType.ExponentialMovingAverage => Domain.Models.StatType.ExponentialMovingAverage,
                        StatType.MovingAverage => Domain.Models.StatType.MovingAverage,
                        StatType.MovingAverageConvergenceDivergence => Domain.Models.StatType.MovingAverageConvergenceDivergence,
                        StatType.RelativeStrengthIndex => Domain.Models.StatType.RelativeStrengthIndex,
                        _ => throw new InvalidCastException($"Unknown StatType {t.StatType}")
                    },
                    t.Value!.Value
                )).ToList()
            )).ToList();

        return new CreateStrategyPayload(
            request.Title!,
            request.Description,
            convertedStages,
            convertedTransitions,
            userId
        );
    }
}