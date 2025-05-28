using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TradeWiseBackend.Api.Models;
using TradeWiseBackend.Api.Responses.v1;
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

        var userModel = new AccountEntityModel
        {
            Id = user.Id,
            Email = user.Email!
        };

        var accessToken = _tokenService.GenerateToken(userModel);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var validation = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (!validation.IsValid || validation.UserId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(validation.UserId);
        if (user == null) return Unauthorized();

        var userModel = new AccountEntityModel
        {
            Id = user.Id,
            Email = user.Email!
        };

        var newAccessToken = _tokenService.GenerateToken(userModel);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        return Ok(new RefreshTokenResponse
        (
            newAccessToken,
            newRefreshToken
        ));
    }
}