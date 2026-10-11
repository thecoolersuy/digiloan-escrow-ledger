using DigiLoan.Application.Common.Models;
using DigiLoan.Application.Features.Account;

namespace DigiLoan.Application.Interfaces;

public interface ITransactionHistoryService
{
    Task<PaginationResult<LedgerEntryDto>> GetPaginationAsync(Guid userId, TransactionQuery query);
}