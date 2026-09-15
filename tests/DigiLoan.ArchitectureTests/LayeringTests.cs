using NetArchTest.Rules;
using Xunit;

namespace DigiLoan.ArchitectureTests;

public class LayeringTests
{
    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherLayers()
    {
        var result = Types.InAssembly(typeof(Domain.Entities.UserAccount).Assembly)
        .Should()
        .NotHaveDependencyOnAny("DigiLoan.Application", "DigiLoan.Infrastructure", "DigiLoan.Api")
        .GetResult();

        Assert.True(result.IsSuccessful, $"Domain has forbidden dependencies on: {string.Join(",", result.FailingTypes)}");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(Application.Services.CreditScoringService).Assembly)
          .Should()
          .NotHaveDependencyOnAny("Domain.Infrastructure", "Domain.Api")
          .GetResult();

        Assert.True(result.IsSuccessful, $"Infrastructure has dependencies on {string.Join(",", result.FailingTypes)}");
    }
}