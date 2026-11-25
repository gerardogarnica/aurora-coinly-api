---
name: unit-testing
description: Generates unit tests for the Domain layer with xUnit and FluentAssertions — entity invariants, state transitions, domain events, value objects and domain services. Use when the user asks to write, fix, or extend tests, wants coverage for a new entity or domain method, reports a failing test, or after adding domain logic that needs regression coverage.
argument-hint: <what to test, e.g. "Wallet.AssignToSavings" or "the Budget period rules">
---

# Unit Testing Skill – Domain Layer

## Overview

`test/Aurora.Coinly.Domain.UnitTests` is the only unit test project. It covers:

- Entities (aggregate roots and their children, through the root)
- Value objects (`Money`, `Color`, `DateRange`...)
- Domain services (`BudgetPeriodService`)

There is **no Application test project**: handlers and validators have no unit tests yet. Creating one means adding NSubstitute (or another mocking library) and an async `DbSet` helper — a new project and a new dependency, so it goes through `spec` → `write-plan`, not into an ad-hoc test. Until then, keep business rules in the domain, where these tests reach them.

The goal is:
- Deterministic tests
- Full business rule coverage
- Zero infrastructure dependencies

## Project structure

```
test/Aurora.Coinly.Domain.UnitTests/
├── Abstractions/
│   └── BaseTest.cs                  → AssertDomainEventWasPublished<T>
├── {Aggregate}/
│   ├── {Entity}Data.cs              → WalletData, UserData, CategoryData
│   ├── {Entity}Tests.cs             → WalletTests
│   └── {DomainService}Tests.cs      → BudgetPeriodServiceTests
├── GlobalUsings.cs
└── Aurora.Coinly.Domain.UnitTests.csproj
```

The folder mirrors the Domain aggregate folder (`Wallets/`, `Budgets/`), so the namespace is `Aurora.Coinly.Domain.UnitTests.{Aggregate}`.

## Naming conventions

| Item | Convention | Example |
|------|------------|---------|
| Test class | `{Entity}Tests`, inheriting `BaseTest` | `WalletTests : BaseTest` |
| Test method | `{Method}_Should_{ExpectedBehavior}[_When{Condition}]` | `Create_Should_SetProperties`, `Update_Should_Fail_WhenIsDeleted` |
| Data class | `internal static class {Entity}Data` | `WalletData` |
| Variables | camelCase | `wallet`, `result` |

A test class is flat: one class per entity, every method under test as a group of facts named after it.

## Test data

Build real instances through their factory methods, from a static data class. This is `Wallets/WalletData.cs` with the fixed `OpenedOn`/`CreatedOnUtc` constants recommended below (the current file passes `DateTime.UtcNow`):

```csharp
namespace Aurora.Coinly.Domain.UnitTests.Wallets;

internal static class WalletData
{
    public const string Name = "Name of the wallet";
    public static readonly Money AvailableAmount = new(100.0m, Currency.Usd);
    public const WalletType Type = WalletType.Cash;
    public const bool AllowNegative = false;
    public static readonly Color Color = Color.FromHex("#000000").Value;
    public const string? Notes = "Notes of the wallet";
    public static readonly DateOnly OpenedOn = new(2026, 1, 1);
    public static readonly DateTime CreatedOnUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Wallet GetWallet() => Wallet.Create(
        UserData.GetUser().Id,
        Name,
        AvailableAmount,
        Type,
        AllowNegative,
        Color,
        Notes,
        OpenedOn,
        CreatedOnUtc);
}
```

- **Prefer fixed dates** for new fixtures — `new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)`, `new DateOnly(2026, 1, 1)` — exposed as constants on the data class. Existing fixtures still pass `DateTime.UtcNow`; that works because domain methods take the timestamp as a parameter, but a test that asserts a date or crosses a month boundary must use a fixed one.
- Value objects come from their real factories: `Color.FromHex("#000000").Value` (the factory returns `Result<Color>`), `new Money(100m, Currency.Usd)`. Their own tests (`ColorTests`, `DateRangeTests`, `PasswordTests`) assert both the success and each `{ValueObject}Errors` failure.
- Entities reach a non-initial state by calling the aggregate's own methods (`wallet.Delete(...)`), never by mutation.
- Data classes may depend on each other across aggregates (`WalletData` uses `UserData`). Aggregate namespaces are in `GlobalUsings.cs`; the test-side data namespaces (`Aurora.Coinly.Domain.UnitTests.Users`) are imported per file.

## Rules

### 1. Test business invariants

Validate required state, constraints and every state transition, including the refusals:

```csharp
[Fact]
public void Update_Should_Fail_WhenIsDeleted()
{
    // Arrange
    var wallet = WalletData.GetWallet();
    wallet.Delete(WalletData.CreatedOnUtc);

    // Act
    var result = wallet.Update(WalletData.Name, true, WalletData.Color, null, WalletData.CreatedOnUtc);

    // Assert
    result.IsSuccessful.Should().BeFalse();
    result.Error.Should().Be(WalletErrors.IsDeleted);
}
```

### 2. Result assertions

The property is `IsSuccessful`, not `IsSuccess`. Compare against the `BaseError` instance itself, never against its code or message string:

```csharp
result.IsSuccessful.Should().BeFalse();
result.Error.Should().Be(WalletErrors.UnableToAssignToSavings);
```

### 3. Domain events

`AssertDomainEventWasPublished<T>(entity)` (from `BaseTest`) asserts the event exists and is unique, then returns it — use the return value to check the payload:

```csharp
var result = wallet.AssignToSavings(amount, assignedOn, updatedOnUtc);

result.IsSuccessful.Should().BeTrue();

var domainEvent = AssertDomainEventWasPublished<WalletSavingsUpdatedEvent>(wallet);
domainEvent.WalletId.Should().Be(wallet.Id);
domainEvent.Amount.Should().Be(amount);
domainEvent.IsIncrement.Should().BeTrue();
```

### 4. No mocking

Domain types have no dependencies to mock. No mocking library, fakes or database.

### 5. Value object equality

```csharp
wallet.SavingsAmount.Should().Be(Money.Zero(WalletData.AvailableAmount.Currency));
```

### 6. Money and currencies

Exercise the currency rules explicitly: `Money` operators throw `InvalidOperationException` when currencies differ, and aggregates return `{Entity}Errors.CurrenciesNotMatch` before getting there. Cover both the matching and the mismatching case.

## Guidelines

- Arrange-Act-Assert with the three comments, as the existing tests do.
- **Prefer `[Fact]`.** The suite is all `[Fact]`: one named scenario per test (`GeneratePeriods_Should_ReturnWeeklyPeriods`, `..._ReturnMonthlyPeriods`) reads better than a parameter table.
- Watch for SonarAnalyzer violations before declaring tests complete — warnings are errors here.
- Use the `{Entity}Data` classes; never build an entity inline when a builder exists, and add a builder method when a new state is needed.
- Tests must be fast and independent of each other.

## Tools & libraries

The stack is fixed — do not introduce others:
- xUnit v2 (2.9.3)
- FluentAssertions 8

`GlobalUsings.cs` imports FluentAssertions, the Domain abstractions and shared namespaces, the aggregate namespaces and `Domain.UnitTests.Abstractions`; `Xunit` comes from the `.csproj` (`<Using Include="Xunit" />`). Don't add per-file `using` directives for them. (`Aurora.Coinly.Domain.Wallets` is not in `GlobalUsings.cs`; `WalletTests` imports it explicitly.)

## Running

```bash
dotnet build "Aurora Coinly.slnx"
dotnet test test/Aurora.Coinly.Domain.UnitTests --no-build --filter "FullyQualifiedName~Aurora.Coinly.Domain.UnitTests.Wallets.WalletTests"
```

The final verification is the `dotnet-test-runner` agent.

## Related skills

- `domain-entity` — entities, value objects, domain events and their EF mapping
- `create-feature` — command/query slices and endpoints
