using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Services;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;
using Moq;

namespace DigiLoan.UnitTests.Services;

public class LoanHistoryServiceTests
{

    private readonly Mock<ILoanApplicationRepository> _mockApplicationRepository = new();
    private readonly Mock<IUserAccountRepository> _mockAccountRepository = new();

    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();

    private readonly Guid _userId = Guid.NewGuid();

    private readonly Guid _accountId = Guid.NewGuid();

    public LoanHistoryService CreateService() => new(
        _mockApplicationRepository.Object,
        _mockAccountRepository.Object,
        _mockUnitOfWork.Object
    );

    [Fact]
    public async Task GetLoanApplicationsAsync_FlipsPastDueLoanToOverdueStatus()
    {
        var account = new UserAccount
        {
            UserId = _userId,
            Id = _accountId,
            CurrentBalance = 10000
        };

        var application = new LoanApplication
        {
            UserAccountId = _accountId,
            Status = LoanStatus.Disbursed,
            RepaymentDate = DateTime.UtcNow.AddDays(-1)
        };
        _mockAccountRepository.Setup(e => e.GetByUserIdAsync(_userId)).ReturnsAsync(account);
        _mockApplicationRepository.Setup(e => e.GetByAccountIdAsync(_accountId)).ReturnsAsync(new List<LoanApplication> { application });

        var service = CreateService();
        var result = await service.GetLoanApplicationsAsync(_userId);

        Assert.Equal(LoanStatus.Overdue, result[0].Status);
        _mockUnitOfWork.Verify(e => e.SaveChangesAsync(), Times.Once);
    }
}