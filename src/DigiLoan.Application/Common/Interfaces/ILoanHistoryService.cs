using DigiLoan.Application.Features.LoanEligibility;

namespace DigiLoan.Application.Interfaces;

public interface ILoanHistoryService
{
    Task<List<LoanApplicationDto>> GetLoanApplicationsAsync(Guid userId);
}