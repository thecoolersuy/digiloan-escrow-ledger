using System.ComponentModel.DataAnnotations;

namespace DigiLoan.Application.Features.Account;

public class TransactionQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}