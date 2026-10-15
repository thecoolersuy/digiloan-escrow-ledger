namespace DigiLoan.Application.Features.LoanEligibility;

public class LoanQuoteResult
{
    public decimal RequestedAmount { get; set; }
    public int TenorMonths { get; set; }
    public decimal InterestPercentage { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal RepaymentAmount { get; set; }
    public DateTime RepaymentDate { get; set; }
}