using DigiLoan.Domain.Enums;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Common;
using Microsoft.Extensions.Logging;


namespace DigiLoan.Application.Services;

public class LoanDisbursementService : ILoanDisbursementService
{
    private readonly IUserAccountRepository _account;
    private readonly ILoanApplicationRepository _application;

    private readonly ILedgerEntryRepository _ledger;

    private readonly ICreditScoringService _creditScoringService;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<LoanDisbursementService> _logger;



    public LoanDisbursementService(IUserAccountRepository account, ILoanApplicationRepository application, ILedgerEntryRepository ledger, ICreditScoringService creditScoringService, IUnitOfWork unitOfWork, ILogger<LoanDisbursementService> logger)
    {
        _account = account;
        _application = application;
        _ledger = ledger;
        _creditScoringService = creditScoringService;
        _unitOfWork = unitOfWork;
        _logger = logger;

    }

    public async Task<LoanApplicationResult> DisburseLoanAsync(Guid userId, LoanApplicationRequest request)
    {
        var account = await _account.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No accounts found for this user");
        var loanApplication = await _application.GetByAccountIdAsync(account.Id);

        var existingLoan = loanApplication.FirstOrDefault(e => e.Status == LoanStatus.Disbursed || e.Status == LoanStatus.Overdue);

        if (existingLoan is not null)
        {
            if (existingLoan.Status == LoanStatus.Disbursed && existingLoan.RepaymentDate > DateTime.UtcNow)
            {
                existingLoan.Status = LoanStatus.Overdue;
                await _application.UpdateAsync(existingLoan);
                await _unitOfWork.SaveChangesAsync();
            }
            throw new InvalidOperationException(
                "You already have an active loan. New applicationsaren't allowed until its resolved."
            );
        }
        EligibilityResult result = await _creditScoringService.EvaluateResultAsync(userId);

        if (!result.IsEligible)
        {
            throw new InvalidOperationException($"You are not currently eligible for a loan. Reasons:[ {string.Join("", result.Reasons)} ]");
        }
        if (request.RequestedAmount > result.MaxApprovedAmount)
        {
            throw new InvalidOperationException($"Requested amount ({request.RequestedAmount}) exceeds your pre-approved limit ({result.MaxApprovedAmount})");
        }
        if (request.RequestedLoanTenor > result.MaxLoanTenor)
        {
            throw new InvalidOperationException(
                    $"Requested tenor ({request.RequestedLoanTenor} months) exceeds your allowed Tenor ({result.MaxLoanTenor})"
            );
        }

        var repaymentAmount = Math.Round(request.RequestedAmount + request.RequestedAmount * (result.InterestRate / 100) * (request.RequestedLoanTenor / 12m));

        var repaymentDate = DateTime.UtcNow.AddMonths(request.RequestedLoanTenor);

        var application = new LoanApplication
        {
            UserAccountId = account.Id,
            UserAccount = account,
            RequestedAmount = request.RequestedAmount,
            RiskScore = result.CreditScore,
            Status = LoanStatus.Pending,
            RepaymentAmount = repaymentAmount,
            RepaymentDate = repaymentDate
        };

        await _application.AddAsync(application);
        _logger.LogInformation("Loan application {LoanApplication} created for account {AccountId}", application.Id, account.Id);
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

        _logger.LogInformation("Loan Application {ApplicationId} disbursed.", application.Id);
        return new LoanApplicationResult
        {
            LoanApplicationId = application.Id,
            DisbursedAmount = request.RequestedAmount,
            NewBalance = account.CurrentBalance,
            DisbursedAtUtc = DateTime.UtcNow,
            RepaymentAmount = repaymentAmount,
            RepaymentDate = repaymentDate,
            Status = application.Status,
        };
    }

}
