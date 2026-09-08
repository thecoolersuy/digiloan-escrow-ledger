namespace DigiLoan.Application.Features.LoanEligibility;

public class LoanApplicationResult
{
    public Guid LoanApplicationId { get; set; }
    public decimal DisbursedAmount { get; set; }

    public decimal NewBalance { get; set; }

    public DateTime DisbursedAtUtc { get; set; }
}