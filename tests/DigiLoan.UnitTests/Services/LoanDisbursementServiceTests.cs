using Castle.Core.Logging;
using DigiLoan.Application.Common.Interfaces;
using Moq;
using DigiLoan.Application.Services;

using Microsoft.Extensions.Logging;
using DigiLoan.Domain.Entities;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Application.Common.Exceptions;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;



namespace DigiLoan.UnitTests.Services;


public class LoanDisbursementServiceTests
{
    private readonly Mock<IUserAccountRepository> _mockAccount = new();
    private readonly Mock<ILedgerEntryRepository> _mockLedger = new();

    private readonly Mock<ILoanApplicationRepository> _mockApplication = new();

    private readonly Mock<ICreditScoringService> _mockCreditScoring = new();

    private readonly Mock<IUnitOfWork> _mockUnitWork = new();

    private readonly Mock<ILogger<LoanDisbursementService>> _mockLogger = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();

    public LoanDisbursementService CreateService() => new(
       _mockAccount.Object,
       _mockApplication.Object,
       _mockLedger.Object,
       _mockCreditScoring.Object,
       _mockUnitWork.Object,
       _mockLogger.Object
    );


    [Fact]
    public async Task DisburseLoanAsync_WhenIneligible_ThrowsInvalidOperationException()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId,
        };
        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockApplication.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication>());

        _mockCreditScoring.Setup(e => e.EvaluateResultAsync(_userId)).ReturnsAsync(new EligibilityResult { IsEligible = false, MaxApprovedAmount = 0m, MaxLoanTenor = 10 });

        var application = new LoanApplicationRequest
        {
            RequestedAmount = 20000m,
            RequestedLoanTenor = 11

        };

        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DisburseLoanAsync(_userId, application));

        _mockLedger.Verify(e => e.AddAsync(It.IsAny<LedgerEntry>()), Times.Never);

    }

    [Fact]
    public async Task DisburseLoanAsync_WhenRequestedMoreThanMaxApproved_ThrowsInvalidOperationException()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId,
        };

        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockApplication.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication>());
        _mockCreditScoring.Setup(e => e.EvaluateResultAsync(_userId)).ReturnsAsync(new EligibilityResult { IsEligible = false, MaxApprovedAmount = 15000m, MaxLoanTenor = 10 });

        var request = new LoanApplicationRequest
        {
            RequestedAmount = 20000m,
            RequestedLoanTenor = 9

        };

        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DisburseLoanAsync(_userId, request));

        _mockLedger.Verify(e => e.AddAsync(It.IsAny<LedgerEntry>()), Times.Never);
    }

    [Fact]
    public async Task DisburseLoanAsync_WhenEligibleWithinLimit_DisburseLoan()
    {
        var account = new UserAccount
        {
            Id = _accountId,
            UserId = _userId,
            CurrentBalance = 20000m
        };

        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockApplication.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication>());

        _mockCreditScoring.Setup(e => e.EvaluateResultAsync(_userId)).ReturnsAsync(new EligibilityResult { IsEligible = true, MaxApprovedAmount = 20000m, MaxLoanTenor = 10 });

        _mockUnitWork.Setup(e => e.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                     .Returns<Func<Task>>(operation => operation());

        var request = new LoanApplicationRequest
        {
            RequestedAmount = 15000m,
            RequestedLoanTenor = 9
        };

        var service = CreateService();
        var result = await service.DisburseLoanAsync(_userId, request);

        Assert.Equal(15000m, result.DisbursedAmount);
        Assert.Equal(35000m, result.NewBalance);
        Assert.Equal(35000m, account.CurrentBalance);

        _mockLedger.Verify(e => e.AddAsync(It.Is<LedgerEntry>(e => e.Amount == 15000m)), Times.Once);
    }

    [Fact]
    public async Task DisburseLoanAsync_WhenConcurrencyExceptionOccurs_ThrowsConcurrencyException()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId
        };

        _mockAccount.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockApplication.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication>());
        _mockCreditScoring.Setup(s => s.EvaluateResultAsync(_userId))
            .ReturnsAsync(new EligibilityResult { IsEligible = true, MaxApprovedAmount = 10000m, MaxLoanTenor = 12 });

        _mockUnitWork.Setup(e => e.ExecuteInTransactionAsync(It.IsAny<Func<Task>>())).ThrowsAsync(new ConcurrencyException("The data was modified by another request"));

        var service = CreateService();

        var request = new LoanApplicationRequest
        {
            RequestedAmount = 10000m,
            RequestedLoanTenor = 9
        };
        await Assert.ThrowsAsync<ConcurrencyException>(() => service.DisburseLoanAsync(_userId, request));
    }

    [Fact]
    public async Task DisburseLoanAsync_WhenRequestedLoanTenorExceededMaxLoanTenor_ThrowsInvalidOperationException()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId
        };
        _mockApplication.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication>());

        _mockCreditScoring.Setup(e => e.EvaluateResultAsync(_userId)).ReturnsAsync(new EligibilityResult
        {
            IsEligible = true,
            MaxApprovedAmount = 15000m,
            MaxLoanTenor = 9
        });

        var application = new LoanApplicationRequest
        {
            RequestedAmount = 12000m,
            RequestedLoanTenor = 12
        };

        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DisburseLoanAsync(_userId, application));

        _mockApplication.Verify(e => e.AddAsync(It.IsAny<LoanApplication>()), Times.Never);
    }



}