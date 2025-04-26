using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Models;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Api.Responses;
using TradeWiseBackend.Api.Responses.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;
using Grpc.Net.Client;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Authorize]
[Microsoft.AspNetCore.Components.Route(RoutesV1.InvestApi)]
public class InvestApiController(IInvestApiService investApiService) : ControllerBase
{
    [HttpPost("link-invest-api-key-with-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountRequest request,
        CancellationToken ct)
    {
        var result = await investApiService.LinkInvestApiKeyWithAccount(
            request.Adapt<LinkInvestApiKeyWithAccountPayload>(), ct);

        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.StatusCode switch
        {
            Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { error = result.ErrorMessage }),
            Grpc.Core.StatusCode.NotFound => NotFound(new { error = result.ErrorMessage }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage })
        };
    }

    [HttpGet("get-supported-instruments")]
    [ProducesResponseType<GetSupportedInstrumentsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList = await investApiService.GetSupportedInstruments(ct);

        return Ok(instrumentsList.Adapt<List<InstrumentInfo>>());
    }

    [HttpGet("get-instrument-stat")]
    [ProducesResponseType<GetInstrumentStatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInstrumentStat(GetInstrumentStatRequest request, CancellationToken ct)
    {
        var instrumentStat = await investApiService.GetInstrumentStat(request.Adapt<GetInstrumentStatPayload>(), ct);

        return Ok(instrumentStat.Adapt<GetInstrumentStatResponse>());
    }
}