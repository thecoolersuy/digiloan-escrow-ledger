namespace DigiLoan.Application.Features.LoanEligibility;

public class EligibilityResult
{
    public bool IsEligible { get; set; }
    public decimal CreditScore { get; set; }

    public decimal MaxApprovedAmount { get; set; }

    public decimal AverageDailyBalance { get; set; }

    public bool HasSalaryHistory { get; set; }

    public List<string> Reasons { get; set; } = new();

    public int MaxLoanTenor { get; set; }

    public decimal InterestRate { get; set; }
}