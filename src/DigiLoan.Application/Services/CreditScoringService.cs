using System.Transactions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;
using Microsoft.VisualBasic;

namespace DigiLoan.Application.Services;

public class CreditScoringService : ICreditScoringService
{
    private const int LookbackDays = 30;
    private const decimal MinimumAverageDailyBalaance = 50000m;

    private readonly IUserAccountRepository _account;
    private readonly ILedgerEntryRepository _ledger;

    public CreditScoringService(IUserAccountRepository account, ILedgerEntryRepository ledger)
    {
        _account = account;
        _ledger = ledger;
    }

    public async Task<EligibilityResult> EvaluateResultAsync(Guid userId)
    {
        var account = await _account.GetByUserIdAsync(userId)
        ?? throw new InvalidOperationException("No accounts found for this user");
        var transactions = await _ledger.GetByAccountIdAsync(account.Id, 30);

        var result = new EligibilityResult();
        result.HasSalaryHistory = transactions.Any(e => e.DestinationAccountId == account.Id && e.TransactionType == TransactionType.SalaryCredit);

        if (!result.HasSalaryHistory)
        {
            result.Reasons.Add("No Salary Credit found in the last 30 days");
        }
        result.AverageDailyBalance = CalculateAverageDailyBalance(account, transactions);
        if (MinimumAverageDailyBalaance > result.AverageDailyBalance)
        {
            result.Reasons.Add($"Your average daily balance is {result.AverageDailyBalance} which doesnot reach the minimum averagebalance criteria to take a loan.");
        }
        decimal totalInflow = transactions.Where(e => e.DestinationAccountId == account.Id).Sum(e => e.Amount);

    }

    public decimal CalculateAverageDailyBalance(UserAccount account, List<LedgerEntry> transactions)
    {
        decimal runningBalance = 0;
        List<decimal> dailyBalances = [];

        for (int day = LookbackDays; day >= 1; day--)
        {
            var targetDate = DateTime.UtcNow.AddDays(-day).Date;

            var todaysTransactions = transactions.Where(e => e.CreatedAtUtc.Date == targetDate);
            foreach (var transaction in todaysTransactions)
            {
                if (transaction.DestinationAccountId == account.Id)
                {
                    runningBalance += transaction.Amount;
                }
                else if (transaction.SourceAccountId == account.Id)
                {
                    runningBalance -= transaction.Amount;
                }
                dailyBalances.Add(runningBalance);
            }
        }
        return dailyBalances.Count > 0 ? Math.Round(dailyBalances.Average(), 2) : 0m;
    }
}

