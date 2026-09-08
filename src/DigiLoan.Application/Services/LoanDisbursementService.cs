using DigiLoan.Domain.Enums;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;


namespace DigiLoan.Application.Services;

public class LoanDisbursementService : ILoanDisbursementService
{
    private readonly IUserAccountRepository _account;
    private readonly ILoanApplicationRepository _application;

    private readonly ICreditScoringService _creditScoringService;

    public LoanDisbursementService(IUserAccountRepository account, ILoanApplicationRepository application, ICreditScoringService creditScoringService)
    {
        _account = account;
        _application = application;
        _creditScoringService = creditScoringService;
    }

    public async Task<LoanApplicationResult> DisburseLoanAsync(Guid userId, LoanApplicationRequest request)
    {
        var account = await _account.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No accounts found for this user");

        EligibilityResult result = await _creditScoringService.EvaluateResultAsync(userId);

        if (!result.IsEligible)
        {
            throw new InvalidOperationException($"You are not currently eligible for a loan. Reasons:[ {result.Reasons} ]");
        }
        if (request.RequestedAmount > result.MaxApprovedAmount)
        {
            throw new InvalidOperationException($"Requested amount ({request.RequestedAmount}) exceeds your pre-approved limit ({result.MaxApprovedAmount})");
        }
        var application = new LoanApplication
        {
            UserAccountId = account.Id,
            UserAccount = account,
            RequestedAmount = request.RequestedAmount,
            RiskScore = result.CreditScore,
            Status = LoanStatus.Pending
        };

        await _application.AddAsync(application);
    }

}
