using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Models;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Api.Responses;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Authorize]
[Microsoft.AspNetCore.Components.Route(RoutesV1.InvestApi)]
public class InvestApiController(IInvestApiService investApiService) : ControllerBase
{
    [HttpPost("link-invest-api-key-with-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountRequest request,
        CancellationToken ct)
    {
        await investApiService.LinkInvestApiKeyWithAccount(
            request.Adapt<LinkInvestApiKeyWithAccountPayload>(), ct);

        //TODO: return actual result
        return Ok();
    }

    [HttpGet("get-supported-instruments")]
    [ProducesResponseType<GetSupportedInstrumentsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList = await investApiService.GetSupportedInstruments(ct);

        return Ok(instrumentsList.Adapt<List<InstrumentInfo>>());
    }
}