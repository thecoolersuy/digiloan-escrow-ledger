using DigiLoan.Domain.Entities;

namespace DigiLoan.Application.Common.Interfaces;

public interface ILoanApplicationRepository
{
    Task AddAsync(LoanApplication application);

    Task UpdateAsync(LoanApplication application);

    Task DeleteAllForAccountAsync(Guid accountId);

    Task<LoanApplication?> GetByIdAsync(Guid id);

    public Task<List<LoanApplication>> GetByAccountIdAsync(Guid accountId);
}