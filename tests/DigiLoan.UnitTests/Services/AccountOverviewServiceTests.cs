using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Account;
using DigiLoan.Application.Services;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;
using Moq;
using Xunit;

namespace DigiLoan.UnitTests.Services;

public class AccountOverviewServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();

    private readonly Mock<IUserIdentityService> _mockUserIdentityService = new();
    private readonly Mock<IUserAccountRepository> _mockAccountRepository = new();

    private readonly Mock<ILedgerEntryRepository> _mockLedgerRepository = new();

    public AccountOverviewService CreateService() => new(_mockUserIdentityService.Object, _mockAccountRepository.Object, _mockLedgerRepository.Object);

    [Fact]
    public async Task GetOverviewDataAsync_ValidUserAccount_ReturnsCorrectFinancialTotals()
    {
        //Arrange
        var account = new UserAccount
        {
            Id = _accountId,
            UserId = _userId,
            CurrentBalance = 30000m
        };

        var entries = new List<LedgerEntry>
        {
            new(){SourceAccountId = _otherId, DestinationAccountId = account.Id, Amount = 20000m,TransactionType = TransactionType.OpeningBalance},
            new(){SourceAccountId = _otherId, DestinationAccountId = account.Id, Amount = 30000m, TransactionType= TransactionType.SalaryCredit},
            new(){SourceAccountId = account.Id, DestinationAccountId = _otherId, Amount = 30000m, TransactionType = TransactionType.QrPayment},
            new(){SourceAccountId = _otherId, DestinationAccountId = account.Id, Amount = 10000m, TransactionType = TransactionType.LoanDisbursement}
        };

        _mockAccountRepository.Setup(a => a.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockLedgerRepository.Setup(a => a.GetByAccountIdAsync(_accountId, 30)).ReturnsAsync(entries);

        //Act
        var service = CreateService();
        var result = await service.GetOverviewDataAsync(_userId);
        //Assert
        Assert.Equal(30000m, result.CurrentBalance);
        Assert.Equal(10000m, result.TotalDisbursed);
        Assert.Equal(50000m, result.TotalInflow);
        Assert.Equal(30000m, result.TotalOutflow);
        Assert.Equal(result.CurrentBalance, result.TotalInflow + result.TotalDisbursed - result.TotalOutflow);
        Assert.Equal(31, result.DailyTrend.Count);
    }

}