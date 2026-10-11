using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using DigiLoan.Application.Features.Account;
using DigiLoan.Domain.Common;
using DigiLoan.Domain.Entities;

public static class LedgerEntryMapper
{
    public static LedgerEntryDto ToDto(LedgerEntry entry, UserAccount account, string fullName)
    {
        (string Label, string AccountNumber) ResolveAccount(Guid id) =>
          id == account.Id
            ? (fullName, account.AccountNumber)
            : (SystemAccounts.GetLabel(id) ?? "Unknown", SystemAccounts.GetAccountNumber(id));

        var source = ResolveAccount(entry.SourceAccountId);
        var destination = ResolveAccount(entry.DestinationAccountId);
        return new LedgerEntryDto
        {
            DisplayId = $"LED-{entry.Id.ToString()[..6].ToUpper()}",
            CreatedAtUtc = entry.CreatedAtUtc,
            SourceLabel = source.Label,
            SourceAccountNumber = source.AccountNumber,
            DestinationLabel = destination.Label,
            DestinationAccountNumber = destination.AccountNumber,
            Amount = entry.Amount,
            TransactionType = entry.TransactionType

        };
    }
}