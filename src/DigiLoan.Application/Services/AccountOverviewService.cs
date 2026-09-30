using DigiLoan.Application.Features.Account;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Services;

public class AccountOverviewService : IAccountOverviewService
{
    private readonly IUserAccountRepository _account;

    private readonly ILedgerEntryRepository _ledger;

    public AccountOverviewService(IUserAccountRepository account, ILedgerEntryRepository ledger)
    {
        _account = account;
        _ledger = ledger;
    }

    public async Task<AccountOverviewDto> GetOverviewDataAsync(Guid userId)
    {
        var account = await _account.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No account found for this user");
        var entries = await _ledger.GetByAccountIdAsync(account.Id, 30);

        var credit = entries.Where(e => e.DestinationAccountId == account.Id).ToList();

        var debit = entries.Where(e => e.SourceAccountId == account.Id).ToList();

        var disbursed = credit.Where(e => e.TransactionType == TransactionType.LoanDisbursement).Sum(e => e.Amount);

        var inflow = credit.Where(e => e.TransactionType != TransactionType.LoanDisbursement).Sum(e => e.Amount);
        var outflow = debit.Sum(e => e.Amount);

        return new AccountOverviewDto
        {
            AccountNumber = account.AccountNumber,
            CurrentBalance = account.CurrentBalance,
            TotalInflow = inflow,
            TotalOutflow = outflow,
            TotalDisbursed = disbursed,
            DailyTrend = CalculateDailyFlow(entries, account.Id)

        };
    }

    public static List<DailyFlowDto> CalculateDailyFlow(List<LedgerEntry> entries, Guid accountId)
    {
        var dateByDay = entries.ToLookup(e => DateOnly.FromDateTime(e.CreatedAtUtc));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var trend = new List<DailyFlowDto>();

        for (var offset = -30; offset <= 0; offset++)
        {
            var day = today.AddDays(offset);
            var entriesOfThatDay = dateByDay[day].ToList();
            trend.Add(new DailyFlowDto
            {
                Date = day,
                Inflow = entriesOfThatDay.Where(e => e.DestinationAccountId == accountId && e.TransactionType != TransactionType.LoanDisbursement).Sum(e => e.Amount),
                Outflow = entriesOfThatDay.Where(e => e.SourceAccountId == accountId).Sum(e => e.Amount),
            });

        }
        return trend;

    }
}