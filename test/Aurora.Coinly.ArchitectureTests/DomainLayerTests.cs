using Aurora.Coinly.Domain.Abstractions;
using NetArchTest.Rules;
using Shouldly;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    public void ValueObjects_Should_BeSealedRecords()
    {
        List<Type> valueObjectTypes = GetValueObjectTypes();

        List<Type> failingTypes = valueObjectTypes
            .Where(t => !t.IsSealed || !IsRecord(t))
            .ToList();

        valueObjectTypes.ShouldNotBeEmpty();
        failingTypes.ShouldBeEmpty();
    }

    [Fact]
    public void ValueObjects_ShouldNotHave_PublicConstructor()
    {
        // Money is a positional record, whose primary constructor is public by design.
        Type[] positionalRecords = [typeof(Domain.Shared.Money)];

        List<Type> valueObjectTypes = GetValueObjectTypes();

        List<Type> failingTypes = valueObjectTypes
            .Where(t => !positionalRecords.Contains(t))
            .Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0)
            .ToList();

        valueObjectTypes.ShouldNotBeEmpty();
        failingTypes.ShouldBeEmpty();
    }

    [Fact]
    public void Records_Should_ImplementValueObject()
    {
        // Completeness net: a new value object that forgets the marker would otherwise
        // silently drop out of the value object tests instead of failing them.
        List<Type> records = GetPublicDomainTypes()
            .Where(IsRecord)
            .Where(t => t.Namespace != typeof(IValueObject).Namespace)
            .ToList();

        List<Type> failingTypes = records
            .Where(t => !typeof(IDomainEvent).IsAssignableFrom(t))
            .Where(t => !typeof(IValueObject).IsAssignableFrom(t))
            .ToList();

        records.ShouldNotBeEmpty();
        failingTypes.ShouldBeEmpty();
    }

    [Fact]
    public void DomainEvents_Should_HaveEventSuffix()
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

    private static List<Type> GetPublicDomainTypes() => DomainAssembly
        .GetTypes()
        .Where(t => t.IsPublic && !t.IsNested && !t.IsDefined(typeof(CompilerGeneratedAttribute)))
        .ToList();

    private static List<Type> GetValueObjectTypes() => GetPublicDomainTypes()
        .Where(t => !t.IsInterface && typeof(IValueObject).IsAssignableFrom(t))
        .ToList();

    // BeImmutable() reports every record as mutable (init accessors compile to non-readonly
    // backing fields), so a record is detected by its synthesized clone method instead.
    private static bool IsRecord(Type type) =>
        type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) is not null;
}