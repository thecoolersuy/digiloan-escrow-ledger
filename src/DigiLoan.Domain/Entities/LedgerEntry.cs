using DigiLoan.Domain.Common;
using DigiLoan.Domain.Entities;
using DigiLoan.Domain.Enums;

public class LedgerEntry : BaseEntity
{
    public Guid SourceAccountId { get; set; }
    public UserAccount? SourceAccount { get; set; }

    public Guid DestinationAccountId { get; set; }
    public UserAccount? DestinationAccount { get; set; }

    public decimal Amount { get; set; }

    public TransactionType TransactionType { get; set; }

}