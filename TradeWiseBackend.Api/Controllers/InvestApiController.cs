using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Components.Route(RoutesV1.InvestApi)]
public class InvestApiController(IInvestApiService investApiService) : ControllerBase
{
    [HttpPost("link-invest-api-key-with-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountRequest request)
    {
        await investApiService.LinkInvestApiKeyWithAccount(
            request.Adapt<LinkInvestApiKeyWithAccountPayload>());

        return Ok();
    }
}