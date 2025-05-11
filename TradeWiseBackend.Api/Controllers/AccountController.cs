using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Responses.v1;
using TradeWiseBackend.Domain.Interfaces.Services;

namespace TradeWiseBackend.Api.Controllers
{
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
    }
}
