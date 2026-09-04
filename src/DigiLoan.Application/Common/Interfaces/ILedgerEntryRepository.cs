namespace DigiLoan.Application.Common.Interfaces;

public interface ILedgerEntryRepository
{
    Task<List<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int lastNDays);

    Task AddAsync(LedgerEntry ledger);

    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);


    /// fintech products should not have delete ledger entries options as even if the account
    /// is deleted its entries should be there, to maintain balance sheets , this delete method is
    /// only there for deleting the seeded data and reseting howeveer it should not be in deployment

    void DeleteAllForAccountAsync(Guid accountId);

}