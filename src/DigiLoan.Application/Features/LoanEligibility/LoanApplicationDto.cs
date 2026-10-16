using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Features.LoanEligibility;

public record LoanApplicationDto
{
    public DateTime CreatedAtUtc;
    public string Id = string.Empty;
    public decimal RequestedAmount;
    public decimal InterestRate;
    public int LoanTenor;

    public LoanStatus Status;

    public DateTime RepaymentDate;
}
