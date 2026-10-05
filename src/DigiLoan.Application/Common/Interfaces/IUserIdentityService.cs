namespace DigiLoan.Application.Common.Interfaces;

public interface IUserIdentityService
{
    Task<string> GetFullNameAsync(Guid userId);
}