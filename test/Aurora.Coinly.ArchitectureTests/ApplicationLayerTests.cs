using Aurora.Coinly.Application.Abstractions.Messaging;
using NetArchTest.Rules;
using Shouldly;

namespace Aurora.Coinly.ArchitectureTests;

public class ApplicationLayerTests : BaseTest
{
    [Fact]
    public void Command_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .Or()
            .ImplementInterface(typeof(ICommand))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Command_Should_HaveCommandSuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .Or()
            .ImplementInterface(typeof(ICommand))
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void CommandHandler_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void CommandHandler_ShouldNot_BePublic()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .NotBePublic()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void CommandHandler_Should_HaveCommandHandlerSuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .AreNotAbstract()
            .And()
            .DoNotResideInNamespace("Aurora.Coinly.Application.Abstractions.Behaviors")
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("CommandBaseHandler`1")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Query_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Query_Should_HaveQuerySuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void QueryHandler_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void QueryHandler_ShouldNot_BePublic()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .NotBePublic()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void QueryHandler_Should_HaveQueryHandlerSuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And()
            .AreNotAbstract()
            .And()
            .DoNotResideInNamespace("Aurora.Coinly.Application.Abstractions.Behaviors")
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .Or()
            .HaveNameEndingWith("QueryHandler`1")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Validator_Should_HaveValidatorSuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Validator_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Validator_ShouldNot_BePublic()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .NotBePublic()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEventHandlers_Should_BeSealed()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEventHandler<>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEventHandlers_Should_HaveEventHandlerSuffix()
    {
        TestResult testResult = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEventHandler<>))
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("EventHandler")
            .GetResult();

        testResult.IsSuccessful.ShouldBeTrue();
    }
}