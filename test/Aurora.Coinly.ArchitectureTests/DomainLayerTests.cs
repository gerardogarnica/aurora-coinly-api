using Aurora.Coinly.Domain.Abstractions;
using NetArchTest.Rules;
using Shouldly;
using System.Reflection;

namespace Aurora.Coinly.ArchitectureTests;

public class DomainLayerTests : BaseTest
{
    [Fact]
    public void BaseEntities_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(BaseEntity))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void BaseEntities_ShouldHave_PrivateParameterlessConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(BaseEntity))
            .GetTypes();

        List<Type> failingTypes = [];
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic |
                                                                        BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.ShouldBeEmpty();
    }

    [Fact]
    public void BaseEntities_ShouldHave_StaticCreateMethod()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(BaseEntity))
            .GetTypes();

        List<Type> failingTypes = [];
        foreach (Type entityType in entityTypes)
        {
            MethodInfo? createMethod = entityType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            if (createMethod == null)
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.ShouldBeEmpty();
    }

    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEvents_Should_BeEventSuffix()
    {
        TestResult testResult = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }
}