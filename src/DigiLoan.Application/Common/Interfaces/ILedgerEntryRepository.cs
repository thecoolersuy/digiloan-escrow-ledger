using DigiLoan.Application.Features.Account;

namespace DigiLoan.Application.Common.Interfaces;

public interface ILedgerEntryRepository
{
    Task<List<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int lastNDays);

    Task AddAsync(LedgerEntry ledger);

    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);


    Task DeleteAllForAccountAsync(Guid accountId);

    Task<(List<LedgerEntry> Items, int TotalCount)> GetPaginationResultByAccountIdAsync(Guid accountId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize);


}