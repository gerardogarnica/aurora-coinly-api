---
paths:
  - "src/Aurora.Coinly.Domain/**/*.cs"
  - "src/Aurora.Coinly.Infrastructure/Configurations/**/*.cs"
---

# Domain model conventions

## Enums live in their owning aggregate folder

Place enum types in `src/Aurora.Coinly.Domain/{Aggregate}/` next to the entity that owns them (`WalletType`, `WalletHistoryType`, `TransactionStatus`, `BudgetFrequency`, `CategoryGroup`), never in `Shared/`. `Shared/` is only for cross-aggregate value objects (`Money`, `Currency`, `Color`, `DateRange`).

Enums are persisted as strings (`HasConversion<string>().HasMaxLength(40)`), so renaming a member is a data migration, and reordering is free. Adding a member needs no migration.

## Value objects

- A value object is a `sealed record` that implements the marker `IValueObject`, with a private constructor (or a positional record, like `Money`) and a static factory. The architecture tests enforce all three, and fail for any public record in Domain (outside `Abstractions`, other than events) that lacks the marker.
- A factory that validates returns `Result<T>` and never throws: `Color.Create`, `DateRange.Create` and `Password.Create` fail with errors from `ColorErrors`, `DateRangeErrors` and `PasswordErrors`. Handlers check `IsSuccessful` and return the error, even when a validator already covers the input, because query validators don't run (`ValidationBehavior` decorates only commands). Domain code that builds a value from input it controls (`BudgetPeriodService`, `MonthlySummary`) reads `.Value` directly. Validation lives in the factory, not the private constructor, because EF Core materializes owned types through that constructor. `Login` maps a malformed password to `UserErrors.InvalidCredentials`, so the response doesn't reveal why the login failed.
- `IValueObject` is an empty marker, read by the architecture tests and by the outbox's `ValueObjectContractResolver`. Domain references no NuGet package (no Json.NET), so value objects carry no serializer attributes: the resolver lets Json.NET rebuild a value object that travels inside an outbox event (`Money` and its `Currency` in `WalletSavingsUpdatedEvent`) through its private constructor. Keep that constructor's parameter names matching the properties (`code` → `Code`), since Json.NET binds them by name. `Color` reaches the outbox only through `BudgetUpdatedEvent` (`Budget.Category.Color`), which nothing raises today. See `domain-events-outbox.md`.
- `Color` is free text up to `Color.MaxLength` (20): no hex check, no case change. The frontend decides what it means.
- EF Core maps every persisted value object with `OwnsOne`, except `Currency`, which is stored as its 3-letter code with `HasConversion(c => c.Code, code => Currency.FromCode(code))` inside each `Money` owned type.

## Money

- Every amount is a `Money`, never a bare `decimal` on an entity. Columns are `numeric(9, 2)` and validators use `.PrecisionScale(9, 2, true)`.
- `Money` operators throw `InvalidOperationException` when currencies differ. Aggregates check first and return a `Result` (`WalletErrors.CurrenciesNotMatch`), so the exception means a bug.
- The dashboard aggregates in a fixed `USD` (`GetDashboardSummaryQueryHandler.CurrencyCode`). Multi-currency is on the roadmap; until then, a feature that sums across wallets must either filter by currency or say in its spec why it doesn't need to.

## Soft delete

Wallets, categories and payment methods are soft-deleted (`IsDeleted`, `DeletedOnUtc`, a `Delete(DateTime)` method that fails with `{Entity}Errors.IsDeleted` when repeated). Lists hide deleted rows unless the query asks for them (`ShowDeleted`). Other mutators refuse to act on a deleted entity with the same error. Transactions are not deleted but move to `TransactionStatus.Removed`.

## Wallet balances

A wallet holds two balances, `AvailableAmount` and `SavingsAmount`; `TotalAmount` is computed (and `Ignore`d by EF). Every balance change goes through a `Wallet` method that also appends a `WalletHistory` operation, so the history explains the balance. Never change a balance without its history entry.

## Domain events

`sealed class` inheriting `DomainEvent`, suffix `Event`, all of an aggregate's events in `{Entity}DomainEvents.cs`. Carry ids and simple values (`Guid`, `DateOnly`, `bool`, `Money`), never entities: `BudgetUpdatedEvent(Budget budget)` is the existing exception and shouldn't be copied. See `domain-events-outbox.md`.
