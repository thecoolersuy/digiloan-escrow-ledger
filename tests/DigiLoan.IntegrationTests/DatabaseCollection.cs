using Xunit;

namespace DigiLoan.IntegrationTests;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}