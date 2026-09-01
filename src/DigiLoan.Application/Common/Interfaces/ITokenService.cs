namespace DigiLoan.Application.Common.Interfaces;

public interface ITokenInterface
{
    public string GenerateToken(Guid userId, string email);
}

