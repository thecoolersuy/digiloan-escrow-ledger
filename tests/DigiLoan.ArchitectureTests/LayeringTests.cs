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

        Assert.True(result.IsSuccessful, $"Domain has forbidden dependencies on: {string.Join(",", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(Application.Services.CreditScoringService).Assembly)
          .Should()
          .NotHaveDependencyOnAny("DigiLoan.Infrastructure", "DigiLoan.Api")
          .GetResult();

        Assert.True(result.IsSuccessful, $"Application has dependencies on {string.Join(",", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(Infrastructure.Persistence.AppDbContext).Assembly)
         .Should()
         .NotHaveDependencyOnAny("DigiLoan.Api")
         .GetResult();

        Assert.True(result.IsSuccessful, $"Infrastructure has dependencies on ${string.Join(",", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Repositories_ShouldOnlyLiveInInfrastructure()
    {
        var result = Types.InAssembly(typeof(Infrastructure.Persistence.AppDbContext).Assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespace("DigiLoan.Infrastructure.Repositories")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Repository doesnt reside on Infrastructure. Violations in : {string.Join(",", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Controllers_ShouldOnlyResideInApi()
    {
        var result = Types.InAssembly(typeof(Api.Controllers.AuthController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("DigiLoan.Api.Controllers")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Controllers should only reside on Api. Violations in {string.Join(",", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }
}