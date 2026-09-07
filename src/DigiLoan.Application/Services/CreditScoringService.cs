using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Services;

public class CreditScoringService : ICreditScoringService
{
    private const int LookbackDays = 30;

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
        decimal totalInflow = transactions.Where(e => e.DestinationAccountId == account.Id).Sum(e => e.Amount);
        decimal totalOutflow = transactions.Where(e => e.SourceAccountId == account.Id).Sum(e => e.Amount);

        decimal minimumAverageDailyBalance = Math.Max(2000m, totalInflow * 0.25m);

        if (totalOutflow > totalInflow)
        {
            result.Reasons.Add("Your outflow amount is more than your inflow amount.");
        }
        result.AverageDailyBalance = CalculateAverageDailyBalance(account, transactions);
        if (minimumAverageDailyBalance > result.AverageDailyBalance)
        {
            result.Reasons.Add($"Your average daily balance is {result.AverageDailyBalance} which doesnot reach the minimum averagebalance criteria to take a loan.");
        }

        result.IsEligible = result.HasSalaryHistory && result.AverageDailyBalance >= minimumAverageDailyBalance && totalOutflow <= totalInflow;

        result.CreditScore = CalculateCreditScore(result.HasSalaryHistory, result.AverageDailyBalance, totalInflow, totalOutflow, minimumAverageDailyBalance);

        result.MaxApprovedAmount = result.IsEligible ? Math.Round(result.AverageDailyBalance * 0.5m, MidpointRounding.ToZero) : 0m;

        return result;

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
            }
            dailyBalances.Add(runningBalance);
        }
        return dailyBalances.Count > 0 ? Math.Round(dailyBalances.Average(), 2) : 0m;
    }

    public decimal CalculateCreditScore(bool hasSalary, decimal avgDailyBalance, decimal inflow, decimal outflow, decimal minimumAverageDailyBalance)
    {
        decimal score = 0;
        if (hasSalary) score += 40;

        score += Math.Min(avgDailyBalance / minimumAverageDailyBalance, 2m) * 20;

        if (inflow > 0)
        {
            var spendRatio = outflow / inflow;
            score += Math.Max(0, 1 - spendRatio) * 20;
        }

        return Math.Round(Math.Min(score, 100), 0);

    }
}

