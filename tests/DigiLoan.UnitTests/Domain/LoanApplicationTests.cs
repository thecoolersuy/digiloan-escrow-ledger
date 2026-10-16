using DigiLoan.Domain.Common;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;

namespace DigiLoan.UnitTests.Domain;

public class LoanApplicationTests
{
    [Theory]
    [InlineData(LoanStatus.Pending, false)]
    [InlineData(LoanStatus.Approved, false)]
    [InlineData(LoanStatus.Rejected, false)]
    [InlineData(LoanStatus.Disbursed, true)]
    [InlineData(LoanStatus.Overdue, true)]
    public void BlocksNewApplications_DependsOnStatus(LoanStatus status, bool expected)
    {
        Assert.Equal(expected, new LoanApplication { Status = status }.BlocksNewApplications);
    }

    [Fact]
    public void MarkOverdueIfPastDue_WhenPastDue_FlipsStatusandReturnsTrue()
    {
        var loan = new LoanApplication { Status = LoanStatus.Disbursed, RepaymentDate = DateTime.Now.AddDays(-1) };
        Assert.True(loan.MarkOverdueIfPastDueDate(DateTime.UtcNow));
        Assert.Equal(LoanStatus.Overdue, loan.Status);
    }

    [Fact]
    public void MarkOverdueIfPastDue_WhenNotYetDue_ChangeNothing()
    {
        var loan = new LoanApplication { Status = LoanStatus.Disbursed, RepaymentDate = DateTime.Now.AddDays(2) };

        Assert.False(loan.MarkOverdueIfPastDueDate(DateTime.Now));
        Assert.Equal(LoanStatus.Disbursed, loan.Status);
    }
    [Fact]
    public void MarkOverdueIfPastDue_WhenAlreadyOverdue_IsIdempotent()
    {
        var loan = new LoanApplication { Status = LoanStatus.Overdue, RepaymentDate = DateTime.UtcNow.AddDays(-5) };

        Assert.False(loan.MarkOverdueIfPastDueDate(DateTime.UtcNow));
    }

    [Fact]
    public void RepaymentAmount_UsesSimpleInterestProratedByTerm()
    {
        Assert.Equal(720m, RepaymentAmountCalculator.InterestAmount(8000m, 18m, 6));
        Assert.Equal(8720m, RepaymentAmountCalculator.RepaymentAmount(8000m, 18m, 6));
    }
}