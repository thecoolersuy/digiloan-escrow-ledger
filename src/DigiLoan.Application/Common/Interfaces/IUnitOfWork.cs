namespace DigiLoan.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();

    Task ExecuteInTransactionAsync(Func<Task> operation);
}