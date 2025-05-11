using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Dal.Entities;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Api.Controllers;

[ApiController]
[Route(RoutesV1.AuthApi)]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AccountEntity> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AccountEntity> _userManager;

    public AuthController(
        UserManager<AccountEntity> userManager,
        ITokenService tokenService,
        SignInManager<AccountEntity> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized();

        var token = await _tokenService.GenerateToken(new AccountEntityModel
        {
            Id = user.Id,
            Email = user.Email!
        });
        return Ok(new { Token = token });
    }
}

public class LoginModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}