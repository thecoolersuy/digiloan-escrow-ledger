using DigiLoan.Domain.Enums;
using DigiLoan.Domain.Common;

namespace DigiLoan.Domain.Entities;

public class LoanApplication : BaseEntity
{
    public Guid UserAccountId { get; set; }
    public UserAccount? UserAccount { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal RiskScore { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
}