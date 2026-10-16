
using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Features.LoanEligibility;

public record LoanApplicationDto
{
    public DateTime CreatedAtUtc { get; init; }
    public string Id { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal InterestRate { get; init; }
    public int LoanTenor { get; init; }
    public LoanStatus Status { get; init; }
    public DateTime RepaymentDate { get; init; }
}
