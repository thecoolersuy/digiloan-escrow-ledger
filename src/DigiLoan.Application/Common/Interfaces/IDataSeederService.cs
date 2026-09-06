namespace DigiLoan.Application.Common.Interfaces;

public interface IDataSeederService
{
    public Task SeedAsync(Guid userId);
}