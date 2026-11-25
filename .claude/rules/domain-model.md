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
- A factory that validates returns `Result<T>` and never throws: `Color.FromHex`, `DateRange.Create` and `Password.Create` fail with errors from `ColorErrors`, `DateRangeErrors` and `PasswordErrors`. Handlers check `IsSuccessful` and return the error, even when a validator already covers the input, because query validators don't run (`ValidationBehavior` decorates only commands). Domain code that builds a value from input it controls (`BudgetPeriodService`, `MonthlySummary`) reads `.Value` directly. Validation lives in the factory, not the private constructor, because EF Core materializes owned types through that constructor. `Login` maps a malformed password to `UserErrors.InvalidCredentials`, so the response doesn't reveal why the login failed.
- `IValueObject` is an empty marker used only by the architecture tests. Value objects that travel inside an outbox event (`Money` and its `Currency` in `WalletSavingsUpdatedEvent`) must be rebuildable by Json.NET: a positional record, or a private constructor marked with `Newtonsoft.Json.JsonConstructor` (as in `Currency`). System.Text.Json's `[JsonConstructor]` has no effect on the outbox. `CurrencyTests` covers the round trip.
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
