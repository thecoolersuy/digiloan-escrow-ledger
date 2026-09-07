using DigiLoan.Application.Features.LoanEligibility;

namespace DigiLoan.Application.Common.Interfaces;

public interface ICreditScoringService
{
    Task<EligibilityResult> EvaluateResultAsync(Guid userId);
}