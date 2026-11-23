---
paths:
  - "test/Aurora.Coinly.Domain.UnitTests/**/*.cs"
---

# Unit test conventions

- **Domain tests** assert entity behavior, value objects and domain events. Pattern: `{Entity}Data.cs` builders + `{Entity}Tests.cs` facts, one folder per aggregate. Test classes inherit `BaseTest` for `AssertDomainEventWasPublished<T>`. Stack: xUnit **v2** + FluentAssertions 8.
- **There is no Application test project.** Handlers and validators are covered only by the architecture tests and manual checks. Adding one (NSubstitute, an async `DbSet` helper) is a new project and a new dependency: it goes through `spec`, and the `unit-testing` skill gets its Application section then.

## Running

`dotnet test` works (xUnit v2 on VSTest). Filter by class with `--filter "FullyQualifiedName~Aurora.Coinly.Domain.UnitTests.Wallets.WalletTests"`. Build the solution first and pass `--no-build`, or `dotnet test` rebuilds the project and its dependencies.

## Dates

Domain methods take timestamps as parameters, so tests never need a clock. Existing builders pass `DateTime.UtcNow`/`DateTime.Now`; new fixtures and any test that asserts a date, a period or a month boundary (budgets, summaries) use fixed constants on the data class. A test that depends on today's date passes or fails depending on when it runs.

## Assertions

- `result.IsSuccessful` (not `IsSuccess`) and `result.Error.Should().Be({Entity}Errors.X)` — compare the `BaseError` instance, never its code string.
- `AssertDomainEventWasPublished<T>(entity)` returns the event; assert its payload.
- Money: `Should().Be(new Money(...))` (record equality).

## Naming

`{Method}_Should_{ExpectedBehavior}[_When{Condition}]`: `Create_Should_SetProperties`, `Delete_Should_Fail_WhenIsDeleted`. Arrange-Act-Assert with the three comments.
