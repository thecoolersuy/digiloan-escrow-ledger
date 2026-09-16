using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Domain.Entities;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigiLoan.Infrastructure.Repositories;

public class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly AppDbContext _context;

    public LoanApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(LoanApplication application)
    {
        _context.LoanApplications.Add(application);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LoanApplication application)
    {
        _context.LoanApplications.Update(application);
        return Task.CompletedTask;
    }

    public Task<LoanApplication?> GetByIdAsync(Guid id) =>
        _context.LoanApplications.FirstOrDefaultAsync(e => e.Id == id);


    public Task<List<LoanApplication>> GetByAccountIdAsync(Guid accountId) =>
        _context.LoanApplications.Where(e => e.UserAccountId == accountId).OrderBy(e => e.CreatedAtUtc).ToListAsync();


}