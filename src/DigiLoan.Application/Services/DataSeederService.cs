using System.Transactions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Common;

namespace DigiLoan.Application.Services;

public class DataSeederService : IDataSeederService
{
    private readonly IUserAccountRepository _userAccount;
    private readonly ILedgerEntryRepository _ledgerEntry;
    private readonly IUnitOfWork _unitOfWork;


    public DataSeederService(
        IUserAccountRepository userAccount,
        ILedgerEntryRepository ledgerEntry,
        IUnitOfWork unitOfWork
    )
    {
        _userAccount = userAccount;
        _ledgerEntry = ledgerEntry;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(Guid userId)
    {
        var account = await _userAccount.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No account found for this user");

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _ledgerEntry.DeleteAllForAccountAsync(account.Id);

            var random = new Random();
            var entries = new List<LedgerEntry>();

            entries.Add(new LedgerEntry
            {
                SourceAccountId = SystemAccounts.Employer,
                DestinationAccountId = account.Id,
                Amount = 50000m,
                TransactionType = Domain.Enums.TransactionType.SalaryCredit,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-25)
            });

            decimal totalSpent = 0m;
            for (int day = 30; day >= 1; day--)
            {
                var amount = random.Next(100, 2500);
                totalSpent += amount;

                var isUtility = day % 7 == 0;

                entries.Add(new LedgerEntry
                {
                    SourceAccountId = account.Id,
                    DestinationAccountId = isUtility ? SystemAccounts.UtilityProvider : SystemAccounts.MerchantPool,
                    TransactionType = isUtility ? Domain.Enums.TransactionType.UtilityBill : Domain.Enums.TransactionType.QrPayment,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-day)
                });
            }

            await _ledgerEntry.AddRangeAsync(entries);

            account.CurrentBalance = 50000m - totalSpent;
            _userAccount.Update(account);

        });


    }
}