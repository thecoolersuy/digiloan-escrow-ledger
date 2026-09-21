using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Auth;
using DigiLoan.Domain.Entities;
using DigiLoan.Infrastructure.Identity;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DigiLoan.Api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public readonly UserManager<ApplicationUser> _userManager;
    public readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;

    private readonly IDataSeederService _dataSeeder;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        AppDbContext dbContext,
        IDataSeederService dataSeeder
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _dataSeeder = dataSeeder;
    }


    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerData)
    {
        var existing = await _userManager.FindByEmailAsync(registerData.Email);
        if (existing is not null)
        {
            return Conflict("An account with this emial already exists");
        }

        var user = new ApplicationUser
        {
            UserName = registerData.FullName,
            Email = registerData.Email
        };

        var result = await _userManager.CreateAsync(user, registerData.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }
        var account = new UserAccount
        {
            UserId = user.Id,
            AccountNumber = $"ACC-{Random.Shared.Next(100000, 999999)}",
            CurrentBalance = 0

        };
        _dbContext.UserAccounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginData)
    {
        var user = await _userManager.FindByEmailAsync(loginData.Email);
        if (user is null)
        {
            return Unauthorized("Account not found");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginData.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized("Invalid Password");
        }
        var token = _tokenService.GenerateToken(user.Id, loginData.Email);
        return Ok(new AuthResponse
        {
            Token = token,
            Email = loginData.Email,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
        });
    }

    [HttpPost("demo-login")]
    public async Task<IActionResult> DemoLogin([FromQuery] SeedProfile profile)
    {
        var email = $"demo-{profile.ToString().ToLower()}@gmail.com";
        var fullName = $"{profile.ToString().ToLower()}";

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser { UserName = fullName, Email = email };
            await _userManager.CreateAsync(user, "DemoPassword123!@#");

            var account = new UserAccount
            {
                UserId = user.Id,
                AccountNumber = $"DEMO-{profile}",
                CurrentBalance = 0
            };
            _dbContext.UserAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
        }
        await _dataSeeder.SeedAsync(user.Id, profile);
        var token = _tokenService.GenerateToken(user.Id, user.Email!);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
        });


    }


}