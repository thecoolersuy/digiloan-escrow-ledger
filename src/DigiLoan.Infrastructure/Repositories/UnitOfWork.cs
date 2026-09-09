using System.Data;
using DigiLoan.Application.Common.Exceptions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace DigiLoan.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{

    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }
    public Task<int> SaveChangesAsync() =>
        _context.SaveChangesAsync();

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await operation();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DBConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new ConcurrencyException(
                   "The data was modified by another request"
                );
            }
        });
    }

}

