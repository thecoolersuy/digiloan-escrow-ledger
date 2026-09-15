using DigiLoan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;


namespace DigiLoan.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public AppDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseSqlServer(_sqlContainer.GetConnectionString())
           .Options;

        DbContext = new AppDbContext(options);
        await DbContext.Database.MigrateAsync();
        await SystemAccountSeeder.SeedSystemAccountsAsync(DbContext);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
}