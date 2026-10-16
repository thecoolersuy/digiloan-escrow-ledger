using DigiLoan.Domain.Enums;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Common;
using Microsoft.Extensions.Logging;
using DigiLoan.Application.Common.Exceptions;


namespace DigiLoan.Application.Services;

public class LoanDisbursementService : ILoanDisbursementService
{
    private readonly IUserAccountRepository _account;
    private readonly ILoanApplicationRepository _application;

    private readonly ILedgerEntryRepository _ledgerEntryRepository;

    private readonly ICreditScoringService _creditScoringService;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<LoanDisbursementService> _logger;



    public LoanDisbursementService(IUserAccountRepository account, ILoanApplicationRepository application, ILedgerEntryRepository ledgerEntryRepository, ICreditScoringService creditScoringService, IUnitOfWork unitOfWork, ILogger<LoanDisbursementService> logger)
    {
        _account = account;
        _application = application;
        _ledgerEntryRepository = ledgerEntryRepository;
        _creditScoringService = creditScoringService;
        _unitOfWork = unitOfWork;
        _logger = logger;

    }

    public async Task<LoanQuoteResult> QuoteAsync(Guid userId, LoanApplicationRequest request)
    {
        var (_, _, quote) = await ValidateAndQuoteAsync(userId, request);
        return quote;
    }

    public async Task<LoanApplicationResult> DisburseLoanAsync(Guid userId, LoanApplicationRequest request)
    {
        var (account, eligibility, quote) = await ValidateAndQuoteAsync(userId, request);

        var application = new LoanApplication
        {
            UserAccountId = account.Id,
            RequestedAmount = quote.RequestedAmount,
            RiskScore = eligibility.CreditScore,
            Status = LoanStatus.Pending,
            LoanTenor = quote.TenorMonths,
            InterestRate = quote.InterestPercentage,
            RepaymentAmount = quote.RepaymentAmount,
            RepaymentDate = quote.RepaymentDate
        };

        await _application.AddAsync(application);

        _logger.LogInformation("LoanApplication created for account {account.Id} : amount = {application.RequestedAmount}", account.Id, application.RequestedAmount);

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _ledgerEntryRepository.AddAsync(new LedgerEntry
                {
                    SourceAccountId = SystemAccounts.EscrowAccount,
                    DestinationAccountId = account.Id,
                    Amount = quote.RequestedAmount,
                    TransactionType = TransactionType.LoanDisbursement,
                    CreatedAtUtc = DateTime.UtcNow
                });
            });

            account.CurrentBalance += quote.RequestedAmount;
            _account.Update(account);

            application.Status = LoanStatus.Disbursed;
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning("A concurrency conflict occured while processing loan disbursement: {Message}", ex.Message);
        }

        _logger.LogInformation("Loan {ApplicationId} disbursed successfully", application.Id);

        return new LoanApplicationResult
        {
            LoanApplicationId = application.Id,
            DisbursedAmount = quote.RequestedAmount,
            NewBalance = account.CurrentBalance,
            DisbursedAtUtc = DateTime.UtcNow,
            RepaymentAmount = quote.RepaymentAmount,
            RepaymentDate = quote.RepaymentDate,
            Status = application.Status
        };

    }

    private async Task<(UserAccount Account, EligibilityResult Eligibility, LoanQuoteResult Quote)> ValidateAndQuoteAsync(Guid userId, LoanApplicationRequest request)
    {
        var account = await _account.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("No account found for this user.");

        var existing = await _application.GetByAccountIdAsync(account.Id);
        var activeLoan = existing.FirstOrDefault(a => a.BlocksNewApplications);

        if (activeLoan is not null)
        {
            if (activeLoan.MarkOverdueIfPastDueDate(DateTime.UtcNow))
            {
                await _application.UpdateAsync(activeLoan);
                await _unitOfWork.SaveChangesAsync();
            }

            throw new InvalidOperationException(
                "You already have an active loan. New applications aren't allowed until it's resolved.");
        }

        var eligibility = await _creditScoringService.EvaluateResultAsync(userId);

        if (!eligibility.IsEligible)
            throw new InvalidOperationException("You are not currently eligible for a loan.");

        if (request.RequestedAmount > eligibility.MaxApprovedAmount)
            throw new InvalidOperationException(
                $"Requested amount ({request.RequestedAmount:C0}) exceeds your pre-approved limit ({eligibility.MaxApprovedAmount:C0}).");

        if (request.RequestedLoanTenor > eligibility.MaxLoanTenor)
            throw new InvalidOperationException(
                $"Requested term ({request.RequestedLoanTenor} months) exceeds your maximum allowed term ({eligibility.MaxLoanTenor} months).");

        var quote = new LoanQuoteResult
        {
            RequestedAmount = request.RequestedAmount,
            TenorMonths = request.RequestedLoanTenor,
            InterestPercentage = eligibility.InterestRate,
            InterestAmount = RepaymentAmountCalculator.InterestAmount(request.RequestedAmount, eligibility.InterestRate, request.RequestedLoanTenor),
            RepaymentAmount = RepaymentAmountCalculator.RepaymentAmount(request.RequestedAmount, eligibility.InterestRate, request.RequestedLoanTenor),
            RepaymentDate = DateTime.UtcNow.AddMonths(request.RequestedLoanTenor)
        };

        return (account, eligibility, quote);
    }

}
