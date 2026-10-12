using System.Security;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Common.Models;
using DigiLoan.Application.Features.Account;
using DigiLoan.Application.Interfaces;

namespace DigiLoan.Application.Services;

public class TransactionHistoryService : ITransactionHistoryService
{
    private readonly IUserAccountRepository _accountRepository;
    private readonly IUserIdentityService _userIdentity;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;

    public TransactionHistoryService(IUserAccountRepository accountRepository, IUserIdentityService userIdentity, ILedgerEntryRepository ledgerEntryRepository)
    {
        _accountRepository = accountRepository;
        _userIdentity = userIdentity;
        _ledgerEntryRepository = ledgerEntryRepository;
    }
    public async Task<PaginationResult<LedgerEntryDto>> GetPaginationAsync(Guid userId, TransactionQuery query)
    {
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
        {
            throw new InvalidOperationException("Fromm must be earlier than To");
        }

        var account = await _accountRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("No account found for this user");

        var fullName = await _userIdentity.GetFullNameAsync(userId);

        var (entries, totalCount) = await _ledgerEntryRepository.GetPaginationResultByAccountIdAsync(account.Id, query.From?.UtcDateTime, query.To?.UtcDateTime, query.Page, query.PageSize);

        return new PaginationResult<LedgerEntryDto>
        {
            Items = entries.Select(e => LedgerEntryMapper.ToDto(e, account, fullName)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize

        };
    }
}