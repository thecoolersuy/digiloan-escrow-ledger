using DigiLoan.Application.Features.Auth;

namespace DigiLoan.Application.Common.Interfaces;

public interface IUserRepository
{
    public Task<bool> RegisterAsync(RegisterRequest registerData);

    public Task<bool> LoginAsync(LoginRequest loginData);
}