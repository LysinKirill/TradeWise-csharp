using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Domain.Interfaces.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

[ApiController]
[Route(RoutesV1.Accounts)]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
    {
        await userService.RegisterUser(
            request.Adapt<UserRegistrationPayload>());

        return Ok();
    }
}