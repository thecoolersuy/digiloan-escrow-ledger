namespace DigiLoan.Application.Common.Interfaces;

public enum SeedProfile
{
    Saver,
    Spender
}
public interface IDataSeederService
{
    public Task SeedAsync(Guid userId, SeedProfile profile);
}