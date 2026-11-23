# Error catalogs and domain events

## Domain errors

The error type is `BaseError` (`Domain/Abstractions/BaseError.cs`). Build it with a static factory — the record's constructor takes three arguments `(Code, Message, ErrorType)`.

**Pick the factory by semantics** — the category drives the HTTP status the endpoint returns (`Api/Responses/ApiResponses.cs`):

| Factory | `BaseErrorType` | HTTP |
|---|---|---|
| `BaseError.Validation` | `Validation` | 400 |
| `BaseError.NotFound` | `NotFound` | 404 |
| `BaseError.Conflict` | `Conflict` | 409 |
| `BaseError.Failure` | `Failure` | 500 |

There is no `Forbidden`: a row that belongs to another user is `NotFound`, so its existence is not leaked. Adding a new `BaseErrorType` means touching `ApiResponses` too.

Error codes are `"{Entity}.{Reason}"` — the entity name is **singular**: `"Wallet.NotFound"`, not `"Wallets.NotFound"`.

```csharp
// src/Aurora.Coinly.Domain/{Aggregate}/{Entity}Errors.cs
namespace Aurora.Coinly.Domain.{Aggregate};

public static class {Entity}Errors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "{Entity}.NotFound",
        "The {entity} with the specified identifier was not found");

    public static readonly BaseError IsDeleted = BaseError.Validation(
        "{Entity}.IsDeleted",
        "The {entity} is already deleted");

    public static readonly BaseError CurrenciesNotMatch = BaseError.Validation(
        "{Entity}.CurrenciesNotMatch",
        "The currencies do not match");

    public static readonly BaseError AlreadyExists = BaseError.Conflict(
        "{Entity}.AlreadyExists",
        "A {entity} with this name already exists");
}
```

## Domain events

All events of an aggregate live in **one file**, `{Entity}DomainEvents.cs`, next to the entity (`WalletDomainEvents.cs`, `TransactionDomainEvents.cs`). Each is a `sealed class` inheriting `DomainEvent`, which supplies `Id` and `OccurredOnUtc`. The name ends with `Event` — the architecture tests check both.

Events carry **ids and simple values, never entity instances**. The event is serialized into the outbox and read back later, possibly after the entity changed; an entity in the payload is a stale snapshot and drags its whole graph into the JSON. (`BudgetUpdatedEvent(Budget budget)` predates this rule; don't copy it.)

```csharp
// src/Aurora.Coinly.Domain/{Aggregate}/{Entity}DomainEvents.cs
namespace Aurora.Coinly.Domain.{Aggregate};

public sealed class {Entity}CreatedEvent(Guid {entity}Id, Guid userId) : DomainEvent
{
    public Guid {Entity}Id { get; init; } = {entity}Id;
    public Guid UserId { get; init; } = userId;
}

public sealed class {Entity}AmountChangedEvent(Guid {entity}Id, Guid userId, Money amount, DateOnly changedOn) : DomainEvent
{
    public Guid {Entity}Id { get; init; } = {entity}Id;
    public Guid UserId { get; init; } = userId;
    public Money Amount { get; init; } = amount;
    public DateOnly ChangedOn { get; init; } = changedOn;
}
```

Carry the `UserId` whenever the handler will need it: event handlers run inside the outbox job, where there is no `IUserContext`. Small value objects like `Money` are fine (`WalletSavingsUpdatedEvent` carries one).

## How events get published

The aggregate only *records* the event with `AddDomainEvent(...)`. Persisting and dispatching is Infrastructure's job:

1. `InsertOutboxMessagesInterceptor` extracts the events on `SaveChanges` and writes them to `coinly.outbox_messages` in the same transaction.
2. `ProcessOutboxJob` (Quartz) deserializes them and dispatches to every `IDomainEventHandler<TEvent>`.

A command handler must never publish a domain event itself — it calls the aggregate method and saves. Renaming an event class breaks pending outbox rows of the old name; see `.claude/rules/domain-events-outbox.md`.
