using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route(RoutesV1.StrategyApi)]
    public class StrategyController(IStrategyService strategyService) : ControllerBase
    {
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateStrategy(CreateStrategyRequest request,
            CancellationToken ct)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("KEKE " + userId);

            var createPayload = request.Adapt<CreateStrategyPayload>() with { UserId = userId };
            await strategyService.CreateStrategyStages(
                createPayload, ct);

            return Ok();
        }
    }
}
