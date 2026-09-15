using DigiLoan.Application.Services;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;
using DigiLoan.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiLoan.IntegrationTests;

[Collection("Database collection")]
public class DataSeederServiceTests
{
    private readonly DatabaseFixture _fixture;



    public DataSeederServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SeedAsync_WithSaverProfile_CreatesRealisticLedgerHistory()
    {
        //Arrange
        var account = new UserAccount
        {
            UserId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        var dbContext = _fixture.DbContext;

        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var accountRepo = new UserAccountRepository(dbContext);
        var ledgerRepo = new LedgerEntryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var seederService = new DataSeederService(accountRepo, ledgerRepo, unitOfWork, NullLogger<DataSeederService>.Instance);

        //Act
        await seederService.SeedAsync(account.UserId, Application.Common.Interfaces.SeedProfile.Saver);

        //Assert
        var seededAccount = await dbContext.UserAccounts.FindAsync(account.Id);
        var entries = dbContext.LedgerEntries.Where(e => e.SourceAccountId == account.Id || e.DestinationAccountId == account.Id);

        Assert.NotNull(seededAccount);
        Assert.True(seededAccount.CurrentBalance > 0, "Saver profile should always end up in some balance");
        Assert.Contains(entries, e => e.TransactionType == TransactionType.SalaryCredit);
        Assert.True(entries.Count() > 1, "Should have generated  30 days seeded data");
    }
}

