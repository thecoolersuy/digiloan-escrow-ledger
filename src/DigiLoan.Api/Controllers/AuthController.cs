using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Auth;
using DigiLoan.Application.Interfaces;
using DigiLoan.Domain.Entities;
using DigiLoan.Infrastructure.Identity;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace DigiLoan.Api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
       IAuthService authService
    )
    {
        _authService = authService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerData)
    {
        var response = await _authService.RegisterAsync(registerData);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginData)
    {
        var response = await _authService.LoginAsync(loginData);

        return Ok(response);
    }

    [HttpPost("demo-login")]
    public async Task<IActionResult> DemoLogin([FromQuery] SeedProfile profile)
    {
        var response = await _authService.DemoLoginAsync(profile);

        return Ok(response);
    }


}