using Aurora.Coinly.Api.Endpoints;
using NetArchTest.Rules;
using Shouldly;

namespace Aurora.Coinly.ArchitectureTests;

public class ApiLayerTests : BaseTest
{
    [Fact]
    public void Endpoint_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(IBaseEndpoint))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Endpoint_Should_BePublic()
    {
        TestResult testResult = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(IBaseEndpoint))
            .And()
            .AreNotAbstract()
            .Should()
            .BePublic()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }
}