using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Common;
using DigiLoan.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DigiLoan.Application.Services;

public class DataSeederService : IDataSeederService
{
    private readonly IUserAccountRepository _userAccount;
    private readonly ILedgerEntryRepository _ledgerEntry;
    private readonly ILoanApplicationRepository _loanApplication;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<DataSeederService> _logger;


    private const decimal OpeningBalanceAmount = 15000m;


    public DataSeederService(
        IUserAccountRepository userAccount,
        ILedgerEntryRepository ledgerEntry,
        ILoanApplicationRepository loanApplication,
        IUnitOfWork unitOfWork,
        ILogger<DataSeederService> logger
    )
    {
        _userAccount = userAccount;
        _ledgerEntry = ledgerEntry;
        _loanApplication = loanApplication;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(Guid userId, SeedProfile profile)
    {
        var account = await _userAccount.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No account found for this user");
        _logger.LogInformation("Seeding {Profile} profile for account {AccountId}", profile, account.Id);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _ledgerEntry.DeleteAllForAccountAsync(account.Id);
            await _loanApplication.DeleteAllForAccountAsync(account.Id);

            var random = new Random();
            var entries = new List<LedgerEntry>();

            entries.Add(new LedgerEntry
            {
                SourceAccountId = SystemAccounts.OpeningBalance,
                DestinationAccountId = account.Id,
                Amount = OpeningBalanceAmount,
                TransactionType = TransactionType.OpeningBalance,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-30)
            });
            entries.Add(new LedgerEntry
            {
                SourceAccountId = SystemAccounts.Employer,
                DestinationAccountId = account.Id,
                Amount = 50000m,
                TransactionType = TransactionType.SalaryCredit,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-25)
            });

            if (profile is not SeedProfile.Saver and not SeedProfile.Spender)
            {
                throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown seed profile type.");
            }

            decimal runningBalance = 0m;
            for (int day = 30; day >= 1; day--)
            {
                runningBalance += entries
                    .Where(e => e.DestinationAccountId == account.Id
                                && e.CreatedAtUtc.Date == DateTime.UtcNow.AddDays(-day).Date)
                    .Sum(e => e.Amount);

                var desiredSpend = profile == SeedProfile.Saver
                    ? random.Next(100, 801)
                    : day >= 26
                        ? random.Next(2600, 2801)
                        : day >= 22
                            ? random.Next(7500, 8001)
                            : random.Next(700, 851);

                var actualSpend = Math.Min(desiredSpend, runningBalance);

                if (actualSpend > 0)
                {
                    runningBalance -= actualSpend;

                    var isUtility = day % 7 == 0;

                    entries.Add(new LedgerEntry
                    {
                        SourceAccountId = account.Id,
                        DestinationAccountId = isUtility ? SystemAccounts.UtilityProvider : SystemAccounts.MerchantPool,
                        Amount = actualSpend,
                        TransactionType = isUtility ? TransactionType.UtilityBill : TransactionType.QrPayment,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-day)
                    });
                }
            }

            await _ledgerEntry.AddRangeAsync(entries);

            account.CurrentBalance = runningBalance;
            _userAccount.Update(account);
        });
        _logger.LogInformation("Seed complete for account {AccountId}", account.Id);


    }
}