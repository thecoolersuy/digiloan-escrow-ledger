using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Auth;

namespace DigiLoan.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);

    Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
    Task<AuthResponse> DemoLoginAsync(SeedProfile profile);
}