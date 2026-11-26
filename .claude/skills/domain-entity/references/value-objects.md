# Value objects

Value objects are `sealed record` (structural equality) and immutable. They live in `Domain/Shared/` when they're shared across aggregates (`Money`, `Currency`, `Color`, `DateRange`); one that belongs to a single aggregate (like `Email` and `Password`) lives in that aggregate's folder.

Every value object implements the empty marker `IValueObject` (`Domain/Abstractions/IValueObject.cs`). It declares no members; it only lets the architecture tests find value objects (`DomainLayerTests`):

- `ValueObjects_Should_BeSealedRecords`: every `IValueObject` is a `sealed record`.
- `ValueObjects_ShouldNotHave_PublicConstructor`: no public constructor, so the factory is the only way in. `Money` is the one exception: a positional record's primary constructor is public.
- `Records_Should_ImplementValueObject`: the completeness net. Every public record in Domain outside `Abstractions` that isn't a domain event must implement `IValueObject`. A new value object that forgets the marker fails here instead of silently escaping the other two tests.

So a new value object is `public sealed record {Name} : IValueObject` with a private constructor.

## The existing shapes

| Value object | Shape | Built with |
|---|---|---|
| `Money` | positional record `(decimal Amount, Currency Currency)` with arithmetic and comparison operators; mixing currencies throws | `new Money(amount, currency)`, `Money.Zero(currency)` |
| `Currency` | closed set of known currencies (`Currency.All`, `Currency.Usd`, `Currency.Default`); travels in the `Money` of `WalletSavingsUpdatedEvent`, rebuilt by the outbox's `ValueObjectContractResolver` through its private constructor | `Currency.FromCode("USD")` |
| `Color` | free text in `Value` (`"#FF5733"`, `"blue"`, a design token), trimmed, case kept, at most `Color.MaxLength` (20) characters; no format check | `Color.Create("#FF5733")` → `Result<Color>` (`ColorErrors`) |
| `DateRange` | `Start`/`End` dates, start on or before end | `DateRange.Create(start, end)` → `Result<DateRange>` (`DateRangeErrors`) |
| `Email` | trimmed and lowercased address in `Value`, at most `Email.MaxLength` (255) characters; `MailAddress` must parse it back to the same string (no `"Name <a@b.com>"`) and the host needs a dot | `Email.Create(value)` → `Result<Email>` (`EmailErrors`) |
| `Password` | the password **hash** in `Hash` (8–500 characters), never the plain password; the handler hashes with `IPasswordHasher` first, and plain-password rules live in the validators | `Password.Create(hash)` → `Result<Password>` (`PasswordErrors`) |

A factory that validates returns `Result<T>` and never throws. `Money` and `Currency` have nothing to validate: `Currency.FromCode` falls back to `Currency.Default` for an unknown code, and the command validator checks the code against `Currency.All`. `Money` throws only when its operators mix currencies; aggregates check that first, so the exception means a bug.

A handler checks the result and returns its error:

```csharp
Result<Color> color = Color.Create(request.Color);
if (!color.IsSuccessful)
{
    return Result.Fail<Guid>(color.Error);
}
```

Domain code that builds a value from input it controls (`BudgetPeriodService`, `MonthlySummary.GetSummaryPeriod`) reads `.Value` directly. Tests do the same with literal input: `Color.Create("#000000").Value`.

## Template

Every new value object with something to validate follows this shape (`Color` is the reference implementation):

```csharp
// src/Aurora.Coinly.Domain/Shared/{ValueObject}.cs
namespace Aurora.Coinly.Domain.Shared;

public sealed record {ValueObject} : IValueObject
{
    public const int MaxLength = 50;

    public string Value { get; init; }

    private {ValueObject}(string value)
    {
        Value = value;
    }

    public static Result<{ValueObject}> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Fail<{ValueObject}>({ValueObject}Errors.Required);
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return Result.Fail<{ValueObject}>({ValueObject}Errors.TooLong);
        }

        return new {ValueObject}(value);
    }

    public override string ToString() => Value;
}
```

Its error catalog goes in a separate `{ValueObject}Errors.cs` in the same folder, with codes `"{ValueObject}.{Reason}"`.

## Rules

- Private constructor; the static factory is the only way in. The constructor only assigns; validation lives in the factory, because EF Core and the outbox's Json.NET resolver both build the value object through that constructor. Name its parameters after the properties (`value` → `Value`), which both bind by name.
- No serializer or persistence attributes: Domain references no NuGet package.
- Normalize before validating (`Trim()`) so the stored form is canonical and the length check applies to what is stored.
- `public const int MaxLength` when there is a length limit; the EF configuration (`HasMaxLength(Color.MaxLength)`) and the validators reuse it, so the number lives in one place. A string field shared by several commands gets one rule extension in `Application/Abstractions/Validations/RuleBuilderOptionsExtensions.cs` (`MustBeValidColor()`).
- Consumers unwrap with `.Value` only after checking `.IsSuccessful`.

## Persistence

Mapped with `OwnsOne` and an explicit `HasColumnName` (the snake_case convention cannot infer the flattened name). `Currency` inside `Money` is stored by code with `HasConversion(c => c.Code, code => Currency.FromCode(code))`. See [ef-configuration.md](ef-configuration.md).
