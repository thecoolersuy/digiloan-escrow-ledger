using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Entities;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiLoan.Infrastructure.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly AppDbContext _context;


    public UserAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<UserAccount?> GetByUserIdAsync(Guid userId) =>
    _context.UserAccounts.FirstOrDefaultAsync(e => e.UserId == userId);


    public Task<UserAccount?> GetByAccountIdAsync(Guid accountId) =>
     _context.UserAccounts.FirstOrDefaultAsync(e => e.Id == accountId);

    public void Update(UserAccount account) =>
     _context.UserAccounts.Update(account);
}