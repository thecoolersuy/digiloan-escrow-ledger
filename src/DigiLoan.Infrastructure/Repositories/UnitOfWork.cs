using System.Data;
using DigiLoan.Application.Common.Exceptions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DigiLoan.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{

    private readonly AppDbContext _context;
    private readonly ILogger<IUnitOfWork> _logger;

    public UnitOfWork(AppDbContext context, ILogger<IUnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
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
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();

                foreach (var entry in ex.Entries)
                {
                    _logger.LogWarning(
                        ex,
                        "EF Core concurrency conflict on entity {EntityType}. " +
                        "Entity state: {EntityState}",
                        entry.Metadata.Name,
                        entry.State);
                }

                throw new ConcurrencyException(
                    "The data was modified by another request."
                );
            }
            ;
        });
    }

}

