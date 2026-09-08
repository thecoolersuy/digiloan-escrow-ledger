using DigiLoan.Application.Features.LoanEligibility;

namespace DigiLoan.Application.Common.Interfaces;

public interface ILoanDisbursementService
{
    Task<LoanApplicationResult> DisburseLoanAsync(Guid userId, LoanApplicationRequest request);
}