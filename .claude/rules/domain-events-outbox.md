---
paths:
  - "src/Aurora.Coinly.Infrastructure/Outbox/**/*.cs"
  - "src/Aurora.Coinly.Infrastructure/Interceptors/InsertOutboxMessagesInterceptor.cs"
  - "src/Aurora.Coinly.Infrastructure/DomainEvents/**/*.cs"
  - "src/Aurora.Coinly.Application/Abstractions/Messaging/IDomainEventHandler.cs"
  - "src/Aurora.Coinly.Application/**/*EventHandler.cs"
  - "src/Aurora.Coinly.Domain/**/*DomainEvents.cs"
  - "src/Aurora.Coinly.Domain/Abstractions/SerializerSettings.cs"
---

# Domain events and the outbox

## Flow

1. Entities raise events (`BaseEntity.AddDomainEvent`).
2. `InsertOutboxMessagesInterceptor` writes them to `coinly.outbox_messages` in the same `SaveChanges` as the change, so an event exists only if its change was committed. `Type` is the event's class name; `Content` is the Json.NET payload.
3. `ProcessOutboxJob` (Quartz, `[DisallowConcurrentExecution]`, every `Outbox:IntervalInSeconds`) reads up to `Outbox:BatchSize` pending messages `FOR UPDATE`, ordered by `occurred_on_utc`, in one transaction. It dispatches each one to every `IDomainEventHandler<T>` (`DomainEventsDispatcher`, a new DI scope per event) and commits.

Side effects on other aggregates are handled this way: `TransactionPaidEvent` → `UpdateSummaryOnTransactionPaidEventHandler` and `UpdateBudgetOnTransactionPaidEventHandler`; `WalletSavingsUpdatedEvent` → `UpdateSummaryOnSavingsAssignedEventHandler`. They are **eventually consistent**: the monthly summary and the budget lag the transaction by up to one job interval (60 s in `appsettings.Development.json`).

## Handlers

- An event handler builds a command and calls that command's handler (`ICommandHandler<TCommand>` injected); it holds no logic of its own. If the result fails it throws `AuroraCoinlyException`, so the job records the error.
- A handler writes through its own scope's `DbContext`, outside the job's transaction, and commits on its own. Its writes stay even if a later message in the batch fails.
- There is no `IUserContext` in the job: everything the handler needs (the `UserId` above all) must be in the event.

## Delivery: one attempt, failures are recorded, not retried

- A message is marked `is_processed = true` after its attempt **whether it succeeded or not**; on failure `error` keeps the exception (truncated to 4000 characters). There are no retries, no attempt counter and no backoff.
- With several handlers for one event, a failure in one stops the rest (`DispatchAsync` awaits them in sequence), and the ones that already ran are not rolled back.
- A failed message is therefore a lost side effect: a paid transaction whose summary was never updated. Find them with `SELECT * FROM coinly.outbox_messages WHERE is_processed AND error <> '' ORDER BY occurred_on_utc DESC`. Recovering means fixing the cause and setting `is_processed = false` on those rows, which re-runs **every** handler of the event — so handlers must be idempotent before anyone does that.
- Make new handlers idempotent anyway (check before inserting, key on the event `Id`, upsert): adding retries later turns delivery into at-least-once.

## Serialization

- Content is Json.NET with `TypeNameHandling.All` (`Domain/Abstractions/SerializerSettings.cs`), and the job deserializes with the same settings and **no serialization binder**: any `$type` in a row is instantiated. The table is only written by the interceptor, but restricting deserialization to the Domain assembly's `IDomainEvent` types (a `SerializationBinder`) is a pending hardening.
- Keep events flat: ids, dates, enums, numbers, and small value objects like `Money`. An entity in the payload (`BudgetUpdatedEvent(Budget budget)`) serializes its whole graph and is a stale snapshot when the handler reads it.
- `Content` is `varchar(4000)`: a payload that grows past it fails the `SaveChanges` of the business change itself.
- **Renaming or moving an event class breaks pending messages** of the old name (the `$type` no longer resolves and the attempt fails). Drain the outbox first, or keep the old type until it is empty.

## Job conventions and known debt

- The job begins its transaction, enumerates the raw query and dispatches without passing `context.CancellationToken`, and logs Information at the start and end of every run even when there is nothing to do. When touching the job: thread the token through every call, use `ToListAsync`, and log Information only when it processed messages.
- `OutboxOptions` has no validation: `IntervalInSeconds` or `BatchSize` at 0 (a missing `Outbox` section) breaks the trigger or the `LIMIT`. Validate them on start (`> 0`) when touching the options.
