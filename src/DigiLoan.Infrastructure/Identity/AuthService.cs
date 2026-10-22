using DigiLoan.Application.Common.Exceptions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Auth;
using DigiLoan.Application.Interfaces;
using DigiLoan.Domain.Entities;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;


namespace DigiLoan.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;

    private readonly IDataSeederService _dataSeeder;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, AppDbContext dbContext, IDataSeederService dataSeeder)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _dataSeeder = dataSeeder;
    }
    public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
    {
        var existing = await _userManager.FindByEmailAsync(registerRequest.Email);

        if (existing is not null)
        {
            throw new ConflictException("An account with this email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = registerRequest.FullName,
            Email = registerRequest.Email,
            FullName = registerRequest.FullName,
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            throw new IdentityValidationException("User registeration failed", result.Errors.Select(e => e.Description));
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

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };

    }

    public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user is null)
        {
            throw new UnauthorizedException("Account not found");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid Password");
        }
        var token = _tokenService.GenerateToken(user.Id, loginRequest.Email);

        return new AuthResponse
        {
            Token = token,
            Email = loginRequest.Email,
            FullName = user.FullName,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),

        };

    }

    public async Task<AuthResponse> DemoLoginAsync(SeedProfile profile)
    {
        var email = $"demo-{profile.ToString().ToLower()}@gmail.com";
        var fullName = $"{profile.ToString().ToLower()}";

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = fullName, Email = email, FullName = fullName };
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
        else if (string.IsNullOrWhiteSpace(user.FullName))
        {
            user.FullName = fullName;
            await _userManager.UpdateAsync(user);
        }
        await _dataSeeder.SeedAsync(user.Id, user.FullName, profile);
        var token = _tokenService.GenerateToken(user.Id, user.Email!);
        return new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
        };
    }
}