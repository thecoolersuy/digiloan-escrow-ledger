using DigiLoan.Domain.Entities;

namespace DigiLoan.Application.Common.Interfaces;

public interface IUserAccountRepository
{
    public Task<UserAccount?> GetByUserIdAsync(Guid userId);

    public Task<UserAccount?> GetByAccountIdAsync(Guid accountId);

    public void Update(UserAccount account);


}