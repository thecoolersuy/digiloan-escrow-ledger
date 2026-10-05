using DigiLoan.Application.Features.Account;

namespace DigiLoan.Application.Common.Interfaces;

public interface ILedgerEntryRepository
{
    Task<List<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int lastNDays);

    Task AddAsync(LedgerEntry ledger);

    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);


    Task DeleteAllForAccountAsync(Guid accountId);


}