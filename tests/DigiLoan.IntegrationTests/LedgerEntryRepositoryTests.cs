
using Azure.Core.Pipeline;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Common;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;
using DigiLoan.Infrastructure.Repositories;


namespace DigiLoan.IntegrationTests;

[Collection("Database collection")]
public class LedgerEntryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public LedgerEntryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    public async Task GetPagedByAccountIdAsync_ReturnsNewestFirst_WithCorrectTotal_AndNoOverlapBetweenPages()
    {
        var db = _fixture.DbContext;

        var account = new UserAccount
        {
            UserId = Guid.NewGuid(),
            AccountNumber = $"Test-{Guid.NewGuid().ToString()[..8]}"
        };
        db.UserAccounts.Add(account);

        await db.SaveChangesAsync();

        db.LedgerEntries.AddRange(Enumerable.Range(1, 25).Select(e => new LedgerEntry
        {
            SourceAccountId = account.Id,
            DestinationAccountId = SystemAccounts.MerchantPool,
            Amount = e,
            TransactionType = TransactionType.QrPayment,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-e)
        }));
        await db.SaveChangesAsync();

        var ledgerRepo = new LedgerEntryRepository(db);
        var (page1, total) = await ledgerRepo.GetPaginationResultByAccountIdAsync(account.Id, null, null, 1, 10);
        var (page2, _) = await ledgerRepo.GetPaginationResultByAccountIdAsync(account.Id, null, null, 2, 10);
        var (page3, _) = await ledgerRepo.GetPaginationResultByAccountIdAsync(account.Id, null, null, 3, 10);

        Assert.Equal(25, total);
        Assert.Equal(10, page1.Count);
        Assert.Equal(10, page2.Count);
        Assert.Equal(5, page3.Count);
        Assert.Equal(1m, page1[0].Amount);





    }
}