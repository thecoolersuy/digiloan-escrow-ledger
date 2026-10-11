namespace DigiLoan.Application.Common.Models;

public class PaginationResult<LedgerEntryDto>
{
    public List<LedgerEntryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}