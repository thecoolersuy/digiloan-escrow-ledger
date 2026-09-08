using System.ComponentModel.DataAnnotations;

namespace DigiLoan.Application.Features.LoanEligibility;

public class LoanApplicationRequest
{
    [Required, Range(1, double.MaxValue, ErrorMessage = "Requested amount must be greater than zero.")]
    public decimal RequestedAmount { get; set; }
}