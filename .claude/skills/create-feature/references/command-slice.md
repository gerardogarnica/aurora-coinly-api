# Command slice

A command is a state change. Every command slice lives in its own folder:

```
src/Aurora.Coinly.Application/{Feature}/{UseCase}/
├── {Verb}{Entity}Command.cs
├── {Verb}{Entity}CommandHandler.cs
└── {Verb}{Entity}CommandValidator.cs
```

| Interface | Handler returns | Use for |
|---|---|---|
| `ICommand` | `Result` | Update, Delete, state changes |
| `ICommand<T>` | `Result<T>` | Create — returns the new `Guid` |

## Command returning a value

```csharp
// src/Aurora.Coinly.Application/{Feature}/Create/Create{Entity}Command.cs
namespace Aurora.Coinly.Application.{Feature}.Create;

public sealed record Create{Entity}Command(
    string Name,
    string CurrencyCode,
    decimal Amount,
    string Color,
    string? Notes) : ICommand<Guid>;
```

```csharp
// src/Aurora.Coinly.Application/{Feature}/Create/Create{Entity}CommandValidator.cs
namespace Aurora.Coinly.Application.{Feature}.Create;

internal sealed class Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(9, 2, true);

        RuleFor(x => x.Color)
            .NotEmpty()
            .Length(7);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }

    private static bool BeAValidCurrencyCode(string currencyCode) =>
        Currency.All.Contains(Currency.FromCode(currencyCode));
}
```

```csharp
// src/Aurora.Coinly.Application/{Feature}/Create/Create{Entity}CommandHandler.cs
namespace Aurora.Coinly.Application.{Feature}.Create;

internal sealed class Create{Entity}CommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<Create{Entity}Command, Guid>
{
    public async Task<Result<Guid>> Handle(
        Create{Entity}Command request,
        CancellationToken cancellationToken)
    {
        // Create {entity}
        var {entity} = {Entity}.Create(
            userContext.UserId,
            request.Name,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            Color.FromHex(request.Color),
            request.Notes,
            dateTimeService.UtcNow);

        dbContext.{Entities}.Add({entity});

        await dbContext.SaveChangesAsync(cancellationToken);

        return {entity}.Id;
    }
}
```

When `{Entity}.Create` enforces invariants it returns `Result<{Entity}>` (as `Transaction.Create` does); unwrap it:

```csharp
Result<{Entity}> result = {Entity}.Create(...);

if (!result.IsSuccessful)
{
    return Result.Fail<Guid>(result.Error);
}

dbContext.{Entities}.Add(result.Value);
```

## Command without a return value

```csharp
// src/Aurora.Coinly.Application/{Feature}/Update/Update{Entity}Command.cs
namespace Aurora.Coinly.Application.{Feature}.Update;

public sealed record Update{Entity}Command(
    Guid Id,
    string Name,
    string Color,
    string? Notes) : ICommand;
```

```csharp
// src/Aurora.Coinly.Application/{Feature}/Update/Update{Entity}CommandHandler.cs
namespace Aurora.Coinly.Application.{Feature}.Update;

internal sealed class Update{Entity}CommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<Update{Entity}Command>
{
    public async Task<Result> Handle(
        Update{Entity}Command request,
        CancellationToken cancellationToken)
    {
        // Get {entity} — always scoped to the current user
        {Entity}? {entity} = await dbContext
            .{Entities}
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if ({entity} is null)
        {
            return Result.Fail({Entity}Errors.NotFound);
        }

        // Update {entity}
        var result = {entity}.Update(
            request.Name,
            Color.FromHex(request.Color),
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        // The entity is tracked: SaveChangesAsync is enough, no dbContext.{Entities}.Update(...)
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
```

Existing handlers (`UpdateCategoryCommandHandler`, `UpdateBudgetLimitCommandHandler`) still call `dbContext.X.Update(entity)` on the tracked entity. It works today but is redundant; don't copy it into new handlers (see Tracking rules).

## Tracking rules

- **Don't call `Update()` on an entity you loaded from the context.** It is already tracked; `SaveChangesAsync` detects the changes. `Update()` only adds risk: it marks every entity in the graph `Modified` when its key is set.
- **Add a new child with a domain-assigned key explicitly.** EF tracks a new entity found through a navigation as `Modified` when its key already has a value, and the save fails trying to update a row that doesn't exist. `WalletHistory` leaves `Id` unset and is inserted normally. `BudgetPeriod` and `BudgetTransaction` assign `Guid.NewGuid()` in their constructors, so the handler calls `dbContext.BudgetTransactions.Add(child)` (see `AddTransactionToBudgetCommandHandler`). See `.claude/rules/ef-core-queries.md`.
- **An entity you attach to a navigation property must be tracked.** If it is going to be assigned to a navigation, load it *without* `AsNoTracking()`.

## Loading a related aggregate

```csharp
Wallet? wallet = await dbContext
    .Wallets
    .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

if (wallet is null)
{
    return Result.Fail(WalletErrors.NotFound);
}
```

## Validation rules reference

```csharp
// Strings
RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(100);

// Money amounts — match the numeric(9, 2) column
RuleFor(x => x.Amount)
    .GreaterThan(0)
    .PrecisionScale(9, 2, true);

// Guids
RuleFor(x => x.WalletId)
    .NotEmpty();

// Email
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress();

// Conditional
RuleFor(x => x.MaxPaymentDate)
    .NotEmpty()
    .When(x => x.IsRecurring);

// Custom
RuleFor(x => x.EndDate)
    .Must((command, endDate) => endDate >= command.StartDate)
    .WithMessage("'{PropertyName}' must be on or after the start date.");
```

To surface a domain error code from a validator rule, use `.WithBaseError({Entity}Errors.X)` (`Application/Abstractions/Validations/RuleBuilderOptionsExtensions.cs`) instead of repeating its code and message.

Dates relative to today must be evaluated per call, not captured at validator construction:

```csharp
// ❌ WRONG — captures the value when the validator is built
.GreaterThanOrEqualTo(dateTimeService.Today)

// ✅ CORRECT — the lambda overload re-evaluates on every validation
.GreaterThanOrEqualTo(_ => dateTimeService.Today)
```

## Anti-patterns

```csharp
// ❌ WRONG: throwing for a business failure
if (wallet is null)
    throw new AuroraCoinlyException(...);

// ✅ CORRECT: return a Result
if (wallet is null)
{
    return Result.Fail(WalletErrors.NotFound);
}

// ❌ WRONG: loading without the owner filter
Wallet? wallet = await dbContext.Wallets.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

// ✅ CORRECT
Wallet? wallet = await dbContext.Wallets
    .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

// ❌ WRONG: business rule evaluated in the handler
if (wallet.AvailableAmount < amount && !wallet.AllowNegative)
    return Result.Fail(WalletErrors.UnableToAssignToSavings);

// ✅ CORRECT: the aggregate decides
var result = wallet.AssignToSavings(amount, request.AssignedOn, dateTimeService.UtcNow);
if (!result.IsSuccessful)
{
    return Result.Fail(result.Error);
}

// ❌ WRONG: DateTime.UtcNow in a handler
wallet.Update(..., DateTime.UtcNow);

// ✅ CORRECT
wallet.Update(..., dateTimeService.UtcNow);
```
