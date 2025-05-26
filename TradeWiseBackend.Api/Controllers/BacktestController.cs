using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;

namespace TradeWiseBackend.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route(RoutesV1.BacktestApi)]
    public class BacktestController() : ControllerBase
    {
        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StartStrategyBacktesting(BacktestRequest request, CancellationToken ct)
        {
            return Ok();
        }
    }
}
