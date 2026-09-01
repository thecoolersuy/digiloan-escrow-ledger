namespace DigiLoan.Application.Common.Interfaces;

public interface ITokenService
{
    public string GenerateToken(Guid userId, string email);
}

