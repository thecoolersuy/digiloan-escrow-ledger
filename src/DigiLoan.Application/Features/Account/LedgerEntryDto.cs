using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Features.Account;

public class LedgerEntryDto
{
    public string DisplayId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string DestinationLabel { get; set; } = string.Empty;
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;

}