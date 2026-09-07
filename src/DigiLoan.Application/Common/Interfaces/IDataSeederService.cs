using DigiLoan.Domain.Enums;

namespace DigiLoan.Application.Common.Interfaces;

public interface IDataSeederService
{
    public Task SeedAsync(Guid userId, SeedProfile profile);
}