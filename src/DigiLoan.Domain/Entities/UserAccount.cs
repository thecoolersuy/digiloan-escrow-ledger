using System.Diagnostics;
using DigiLoan.Domain.Common;

namespace DigiLoan.Domain.Entities;

public class UserAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<LedgerEntry> OutgoingEntries { get; set; } = new List<LedgerEntry>();
    public ICollection<LedgerEntry> IncomingEntries { get; set; } = new List<LedgerEntry>();

    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();






}