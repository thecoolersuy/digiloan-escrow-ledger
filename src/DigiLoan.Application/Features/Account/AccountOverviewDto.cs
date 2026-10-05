namespace DigiLoan.Application.Features.Account;

public class AccountOverviewDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal TotalInflow { get; set; }
    public decimal TotalOutflow { get; set; }
    public decimal TotalDisbursed { get; set; }
    public List<DailyFlowDto> DailyTrend { get; set; } = new();

    public List<LedgerEntryDto> RecentTransactions { get; set; } = new();
}

public class DailyFlowDto
{
    public DateOnly Date { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
}