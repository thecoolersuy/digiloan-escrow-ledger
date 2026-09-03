namespace DigiLoan.Application.Features.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}