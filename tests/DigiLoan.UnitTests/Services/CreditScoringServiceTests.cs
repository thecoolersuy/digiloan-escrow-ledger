using Castle.Core.Logging;
using DigiLoan.Application.Common.Interfaces;
using Moq;
using DigiLoan.Application.Services;
using Microsoft.Extensions.Logging;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;

namespace DigiLoan.UnitTests.Services;

public class CreditScoringServiceTests
{
    private readonly Mock<IUserAccountRepository> _mockAccount = new();
    private readonly Mock<ILedgerEntryRepository> _mockLedgerEntry = new();
    private readonly Mock<ILogger<CreditScoringService>> _mockLogger = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();

    private CreditScoringService CreateService() =>
    new(_mockAccount.Object, _mockLedgerEntry.Object, _mockLogger.Object);

    [Fact]
    public async Task EvaluateResultAsync_WithNoSalaryHistory_ReturnsIneligible()
    {
        //Arrange
        var account = new UserAccount
        {
            Id = _accountId,
            UserId = _userId,
            CurrentBalance = 10000m
        };

        _mockAccount.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockLedgerEntry.Setup(r => r.GetByAccountIdAsync(_accountId, 30)).ReturnsAsync(new List<LedgerEntry>());

        var service = CreateService();

        //Act
        var result = await service.EvaluateResultAsync(_userId);

        //Assert
        Assert.False(result.IsEligible);
        Assert.False(result.HasSalaryHistory);
        Assert.Contains(result.Reasons, r => r.Contains("salary", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EvaluateResultAsync_WhenOutflowExceedsInflow_ReturnsIneligible()
    {
        //Arrange

        var account = new UserAccount
        {
            Id = _accountId,
            UserId = _userId,
        };
        var entries = new List<LedgerEntry>
        {
            new()
            {
              SourceAccountId =  Guid.NewGuid(),
              DestinationAccountId = account.Id,
              Amount = 50000m,
              TransactionType = TransactionType.SalaryCredit,
              CreatedAtUtc = DateTime.UtcNow.AddDays(-25),
            },
            new()
            {
                SourceAccountId = account.Id,
                DestinationAccountId = Guid.NewGuid(),
                Amount = 60000m,
                TransactionType = TransactionType.QrPayment,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-15),
            }
        };
        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockLedgerEntry.Setup(e => e.GetByAccountIdAsync(_accountId, 30)).ReturnsAsync(entries);

        //Act

        var service = CreateService();
        var result = await service.EvaluateResultAsync(_userId);

        //Assert

        Assert.False(result.IsEligible);
        Assert.Contains(result.Reasons, r => r.Contains("outflow", StringComparison.OrdinalIgnoreCase) || r.Contains("spending", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EvaluateResultAsync_WithHealthyHistory_ReturnsEligible()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId,
        };

        var entries = new List<LedgerEntry>
        {
            new()
            {
               SourceAccountId = Guid.NewGuid(),
               DestinationAccountId = account.Id,
               Amount = 50000m,
               TransactionType = TransactionType.SalaryCredit,
               CreatedAtUtc = DateTime.UtcNow.AddDays(-25),
            },
            new()
            {
                SourceAccountId = account.Id,
                DestinationAccountId = Guid.NewGuid(),
                Amount = 5000m,
                TransactionType = TransactionType.QrPayment,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)

            }
        };

        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockLedgerEntry.Setup(e => e.GetByAccountIdAsync(_accountId, 30)).ReturnsAsync(entries);

        var service = CreateService();
        var result = await service.EvaluateResultAsync(_userId);

        Assert.True(result.IsEligible);
        Assert.True(result.HasSalaryHistory);
        Assert.True(result.MaxApprovedAmount > 0);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public async Task EvaluateResultAsync_WithNoLedgerHistory_DoenotThrowError()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId
        };
        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockLedgerEntry.Setup(e => e.GetByAccountIdAsync(_accountId, 30)).ReturnsAsync(new List<LedgerEntry>());

        var service = CreateService();
        var exception = await Record.ExceptionAsync(() => service.EvaluateResultAsync(_userId));


        Assert.Null(exception);
    }


}