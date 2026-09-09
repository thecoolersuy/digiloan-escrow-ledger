using DigiLoan.Domain.Enums;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Common;
using DigiLoan.Application.Common.Exceptions;


namespace DigiLoan.Application.Services;

public class LoanDisbursementService : ILoanDisbursementService
{
    private readonly IUserAccountRepository _account;
    private readonly ILoanApplicationRepository _application;

    private readonly ILedgerEntryRepository _ledger;

    private readonly ICreditScoringService _creditScoringService;
    private readonly IUnitOfWork _unitOfWork;

    public LoanDisbursementService(IUserAccountRepository account, ILoanApplicationRepository application, ILedgerEntryRepository ledger, ICreditScoringService creditScoringService, IUnitOfWork unitOfWork)
    {
        _account = account;
        _application = application;
        _ledger = ledger;
        _creditScoringService = creditScoringService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanApplicationResult> DisburseLoanAsync(Guid userId, LoanApplicationRequest request)
    {
        var account = await _account.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No accounts found for this user");

        EligibilityResult result = await _creditScoringService.EvaluateResultAsync(userId);

        if (!result.IsEligible)
        {
            throw new InvalidOperationException($"You are not currently eligible for a loan. Reasons:[ {string.Join("", result.Reasons)} ]");
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
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _ledger.AddAsync(new LedgerEntry
            {
                SourceAccountId = SystemAccounts.EscrowAccount,
                DestinationAccountId = account.Id,
                Amount = request.RequestedAmount,
                TransactionType = TransactionType.LoanDisbursement
            });

            account.CurrentBalance += request.RequestedAmount;

            _account.Update(account);

            application.Status = LoanStatus.Disbursed;
        });

        return new LoanApplicationResult
        {
            LoanApplicationId = application.Id,
            DisbursedAmount = request.RequestedAmount,
            NewBalance = account.CurrentBalance,
            DisbursedAtUtc = DateTime.UtcNow
        };
    }

}
