using DigiLoan.Application.Features.Account;

namespace DigiLoan.Application.Common.Interfaces;

public interface IAccountOverviewService
{
    Task<AccountOverviewDto> GetOverviewDataAsync(Guid userId);
}