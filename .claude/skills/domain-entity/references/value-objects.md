# Value objects

Value objects are `sealed record` (structural equality) and immutable. They live in `Domain/Shared/` when they're shared across aggregates (`Money`, `Currency`, `Color`, `DateRange`); one that belongs to a single aggregate (like `Password`) lives in that aggregate's folder.

There is no marker interface for value objects in this codebase, and the architecture tests don't select them.

## The existing shapes

| Value object | Shape | Built with |
|---|---|---|
| `Money` | positional record `(decimal Amount, Currency Currency)` with arithmetic and comparison operators; mixing currencies throws | `new Money(amount, currency)`, `Money.Zero(currency)` |
| `Currency` | closed set of known currencies (`Currency.All`, `Currency.Usd`, `Currency.Default`) | `Currency.FromCode("USD")` |
| `Color` | `Value` holding `#RRGGBB`, upper-cased | `Color.FromHex("#FF5733")` |
| `DateRange` | `Start`/`End` dates | `DateRange.Create(start, end)` |
| `Password` | the plain password, minimum 8 characters (hashing goes through `IPasswordHasher`) | `Password.Create(value)` |

`Color`, `DateRange` and `Password` **throw** (`ArgumentException`) on malformed input, and `Money` throws when its operators mix currencies. That is acceptable only because the command validator rejects bad input first (`Color` has `.Length(7)`, `CurrencyCode` is checked against `Currency.All`), so a throw means a bug, not a user error. Keep that pairing when adding a value object: either the validator guarantees the input, or `Create` returns `Result<T>`.

## Template — returning `Result`

Prefer this for a new value object whose input isn't fully covered by a validator:

```csharp
// src/Aurora.Coinly.Domain/Shared/{ValueObject}.cs
namespace Aurora.Coinly.Domain.Shared;

public sealed record {ValueObject}
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
            return Result.Fail<{ValueObject}>({ValueObject}Errors.Empty);
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

- Private constructor; the static factory is the only way in.
- Normalize before validating (`Trim()`, `ToUpperInvariant()`) so the stored form is canonical — `Color` upper-cases its hex.
- `public const int MaxLength` when there is a length limit; the EF configuration reuses it.
- Consumers unwrap with `.Value` only after checking `.IsSuccessful`.

## Persistence

Mapped with `OwnsOne` and an explicit `HasColumnName` (the snake_case convention cannot infer the flattened name). `Currency` inside `Money` is stored by code with `HasConversion(c => c.Code, code => Currency.FromCode(code))`. See [ef-configuration.md](ef-configuration.md).
