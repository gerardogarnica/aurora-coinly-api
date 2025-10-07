using Aurora.Coinly.Domain.Transactions;
using System.Reflection;

namespace Aurora.Coinly.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Transaction).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(Api.Program).Assembly;
}