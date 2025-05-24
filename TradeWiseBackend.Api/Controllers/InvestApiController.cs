using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Models;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Api.Responses;
using TradeWiseBackend.Api.Responses.models;
using TradeWiseBackend.Api.Responses.v1;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Authorize]
[Route(RoutesV1.InvestApi)]
public class InvestApiController(IInvestApiService investApiService) : ControllerBase
{
    [HttpPost("link-invest-api-key-with-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkInvestApiKeyWithAccount(LinkInvestApiKeyWithAccountRequest request,
        CancellationToken ct)
    {
        await investApiService.LinkInvestApiKeyWithAccount(
            request.Adapt<LinkInvestApiKeyWithAccountPayload>(), ct);

        return Ok();
    }

    [HttpGet("get-supported-instruments")]
    [ProducesResponseType<GetSupportedInstrumentsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedInstruments(CancellationToken ct)
    {
        var instrumentsList = await investApiService.GetSupportedInstruments(ct);

        return Ok(instrumentsList.Adapt<List<InstrumentInfo>>());
    }

    [HttpPost("get-instrument-stat")]
    [ProducesResponseType<GetInstrumentStatResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstrumentStat(GetInstrumentStatRequest request, CancellationToken ct)
    {
        var instrumentStat = await investApiService.GetInstrumentStat(request.Adapt<GetInstrumentStatPayload>(), ct);

        return Ok(instrumentStat.Adapt<GetInstrumentStatResponse>());
    }

    [HttpPost("get-candles-by-instrument")]
    [ProducesResponseType<GetCandlesByInstrumentResponse>(StatusCodes.Status200OK)]
    public async Task<GetCandlesByInstrumentResponse> GetCandlesByInstrument(GetCandlesByInstrumentRequest request, CancellationToken ct)
    {
        var candles = await investApiService.GetCandlesByInstrument(request.Adapt<GetCandlesByInstrumentPayload>(), ct);
        var candlesProtoList = candles.Adapt<List<Candle>>();

        return new GetCandlesByInstrumentResponse(candlesProtoList);
    }

    [HttpGet("get-supported-models")]
    [ProducesResponseType<GetSupportedModelsResponse>(StatusCodes.Status200OK)]
    public async Task<GetSupportedModelsResponse> GetSupportedModels(CancellationToken ct)
    {
        var models = await investApiService.GetSupportedModels(ct);

        return new GetSupportedModelsResponse(models.Adapt<List<Responses.models.Model>>());
    }
}