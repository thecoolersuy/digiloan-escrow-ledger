using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigiLoan.Infrastructure.Repositories;

public class LedgerEntryRepository : ILedgerEntryRepository

{
    private readonly AppDbContext _context;

    public LedgerEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int lastNDays)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-lastNDays);

        return _context.LedgerEntries.Where(e => (e.SourceAccountId == accountId || e.DestinationAccountId == accountId)
                          && e.CreatedAtUtc >= cutoff)
                  .OrderBy(e => e.CreatedAtUtc)
                  .ToListAsync();
    }

    public Task AddAsync(LedgerEntry ledger)
    {
        _context.LedgerEntries.Add(ledger);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<LedgerEntry> entries)
    {
        _context.LedgerEntries.AddRange(entries);
        return Task.CompletedTask;
    }


    public async Task DeleteAllForAccountAsync(Guid accountId)
    {
        var entries = await _context.LedgerEntries.Where(e => e.SourceAccountId == accountId || e.DestinationAccountId == accountId).ToListAsync();

        _context.LedgerEntries.RemoveRange(entries);
    }

    public async Task<(List<LedgerEntry> Items, int TotalCount)> GetPaginationResultByAccountIdAsync(Guid accountId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize)
    {
        var query = _context.LedgerEntries.AsNoTracking().Where(e => e.SourceAccountId == accountId || e.DestinationAccountId == accountId);

        if (fromUtc.HasValue)
        {
            query = query.Where(e => e.CreatedAtUtc >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            query = query.Where(e => e.CreatedAtUtc <= toUtc.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }



}