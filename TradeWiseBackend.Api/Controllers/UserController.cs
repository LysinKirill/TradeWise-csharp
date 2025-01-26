using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Requests.v1;
using TradeWiseBackend.Domain.Interfaces.Interfaces.Services;
using TradeWiseBackend.Domain.ServiceModels;

[ApiController]
[Route(RoutesV1.Auth)]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("user/register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterCustomer(RegisterUserRequest request)
    {
        await userService.RegisterUser(
            request.Adapt<UserRegistrationPayload>());

        return Ok();
    }
}