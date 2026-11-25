---
name: domain-entity
description: Adds or changes a domain entity and maps it to the database — aggregate roots, child entities, value objects, domain events, error catalogs, and the EF Core configuration. Use when the user asks to add or change an entity, aggregate, value object, domain event, or {Entity}Errors class; when a business rule or invariant needs a home; when an entity needs a table, column, relationship, or index; or when asked where domain logic belongs.
argument-hint: <entity and its fields, e.g. "SavingsGoal with a name, target amount and target date">
---

# Domain entity

Write the domain type, then map it. An entity that is never mapped is never persisted, so both steps belong together.

Once the entity exists, continue with the `create-feature` skill for the use case and endpoint.

## Workflow

1. **Classify what you are writing.** Aggregate root, child entity, or value object. See the table below.
2. **Write the domain type** → `src/Aurora.Coinly.Domain/{Aggregate}/`. Templates: [references/aggregate-root.md](references/aggregate-root.md) and [references/value-objects.md](references/value-objects.md).
3. **Write the error catalog and domain events** → `{Entity}Errors.cs` and `{Entity}DomainEvents.cs`. Template: [references/errors-and-events.md](references/errors-and-events.md).
4. **Write the EF configuration** → `src/Aurora.Coinly.Infrastructure/Configurations/{Entity}Configuration.cs`. Template: [references/ef-configuration.md](references/ef-configuration.md). Let *Persistence consequences* below drive what it must contain.
5. **Register the DbSet** — add `DbSet<{Entity}> {Plural}` to **both** `Application/Abstractions/Data/ICoinlyDbContext.cs` and `Infrastructure/Database/ApplicationDbContext.cs`.
6. **Register the namespace** — a new aggregate folder gets a `global using Aurora.Coinly.Domain.{Aggregate};` in `Application/GlobalUsings.cs` (and in the test project's `GlobalUsings.cs` when tests use it).
7. **Create the migration** — plain PascalCase name (`AddSavingsGoals`):

   ```bash
   dotnet ef migrations add {Name} --project src/Aurora.Coinly.Infrastructure --startup-project src/Aurora.Coinly.Api --output-dir Database/Migrations
   ```

   If the change should **not** alter the schema (an `Ignore` for a computed property, a mapping refactor), confirm it instead — after a fresh build, since the check reads the built assemblies:

   ```bash
   dotnet ef migrations has-pending-model-changes --project src/Aurora.Coinly.Infrastructure --startup-project src/Aurora.Coinly.Api
   ```

8. **Write the tests** with the `unit-testing` skill: `{Aggregate}Data.cs` builder and `{Aggregate}Tests.cs`.
9. **Verify** — launch the `dotnet-test-runner` agent (Agent tool, `subagent_type: dotnet-test-runner`). It runs `dotnet build` (warnings are errors here) and both test projects, and reports only failures. The architecture tests check that the entity is `sealed`, has a private parameterless constructor and a public static `Create`, and that its events are `sealed` with the `Event` suffix. Do not consider the entity done until that agent comes back clean.

## What to write

| Concept | Purpose | Base type | Example |
|---------|---------|-----------|---------|
| Aggregate root | Entry point for the aggregate | `BaseEntity` | `Wallet`, `Budget`, `Transaction` |
| Child entity | Part of an aggregate, changed only through the root | none | `WalletHistory`, `BudgetPeriod`, `BudgetTransaction` |
| Value object | Immutable, no identity | `IValueObject` (`sealed record`) | `Money`, `Currency`, `Color`, `DateRange`, `Password` |
| Domain event | Signals a state change other aggregates react to | `DomainEvent` | `WalletSavingsUpdatedEvent` |

Child entities don't inherit `BaseEntity`: they raise no events, declare their own `Guid Id`, and have an `internal static Create(...)` so only the root can build them. Leave the `Id` unset so EF generates it on insert, as `WalletHistory` does. `BudgetPeriod` and `BudgetTransaction` assign it in their constructor, which forces handlers to add new instances explicitly (see [aggregate-root.md](references/aggregate-root.md#child-entity)). The architecture tests don't select child entities, so nothing checks a child's shape: follow `WalletHistory`.

## Folder layout

`{Aggregate}` is the **plural PascalCase folder name** (`Wallets`, `Budgets`, `Transactions`). `{Entity}` is the singular type name.

```
src/Aurora.Coinly.Domain/{Aggregate}/
├── {Entity}.cs                    # Aggregate root
├── {Entity}Errors.cs              # Typed errors
├── {Entity}DomainEvents.cs        # All events of the aggregate (one file)
├── {ChildEntity}.cs               # Child entity (if applicable)
└── {Entity}Status.cs              # Enumerations owned by this aggregate
```

Enums live in their owning aggregate folder, never in `Shared/`. Value objects shared across aggregates go in `Domain/Shared/`.

## Non-negotiable conventions

- **`sealed`, private setters always.** `Id` is inherited from `BaseEntity` — never redeclare it.
- **Private parameterless constructor** `private {Entity}() : base(Guid.NewGuid())`, which also initializes non-nullable reference properties (`Name = string.Empty;`, `Amount = Money.Zero();`). EF uses it; `Create` uses it too.
- **`public static Create(...)`** is the only way in. It returns the entity when creation can't fail, and `Result<{Entity}>` when it enforces invariants (`Transaction.Create`).
- **Mutating methods return `Result` or `Result<{Entity}>`** (`return this;` on success) — never throw for an expected failure.
- **The aggregate root owns its children.** Children are added and changed through methods on the root; invariants that need the root's state are enforced in the root.
- **Collections are private fields** exposed as `IReadOnlyCollection<T>` with `.AsReadOnly()`.
- **Every record belongs to a user.** Aggregate roots carry `Guid UserId`, set in `Create` from `userContext.UserId`; handlers filter by it.
- **Timestamps are parameters** (`DateTime createdOnUtc`, `DateOnly assignedOn`), never `DateTime.UtcNow` inside the entity. That keeps domain tests deterministic.
- **Soft delete** — deletable aggregates have `IsDeleted` / `DeletedOnUtc` and a `Delete(DateTime deletedOnUtc)` method that fails with `{Entity}Errors.IsDeleted` when already deleted; other mutators refuse to act on a deleted entity the same way.
- **Errors are `BaseError`**, built with `BaseError.Validation/NotFound/Conflict/Failure`. Codes are `"{Entity}.{Reason}"`, singular entity: `"Wallet.NotFound"`.
- **Entities stay persistence-ignorant** — no EF attributes in Domain.

## Persistence consequences

Each domain shape forces a specific mapping. Decide these while designing the entity:

| Shape in the domain | Required in the EF configuration |
|---|---|
| `private readonly List<T> _items` exposed as `IReadOnlyCollection<T> Items` | Nothing extra when the field is named `_items` for `Items`: EF finds the backing field by convention. Configure the relationship (`HasMany(...).WithOne().HasForeignKey(...)`) explicitly. |
| `Money` property | `OwnsOne(x => x.Amount, y => { y.Property(m => m.Amount).HasColumnName("amount").HasColumnType("numeric(9, 2)"); y.Property(m => m.Currency).HasConversion(c => c.Code, code => Currency.FromCode(code)).HasMaxLength(3); })` |
| `Color` property | `OwnsOne(x => x.Color, y => y.Property(c => c.Value).HasColumnName("color").HasMaxLength(7))` |
| Enum property | `HasConversion<string>().HasMaxLength(40)` |
| Computed property (`TotalAmount`) | `builder.Ignore(x => x.TotalAmount)` |
| `UserId` on a new aggregate root | `builder.HasIndex(x => x.UserId)`, since every query filters on it (existing tables have none yet) |

## Notes

- Domain events are recorded by the aggregate and written to `coinly.outbox_messages` by `InsertOutboxMessagesInterceptor` on `SaveChanges`; the Quartz `ProcessOutboxJob` dispatches them later. A handler never publishes them itself. See `.claude/rules/domain-events-outbox.md`.
- There is no Application test project: domain tests are the only unit tests, so put the logic where they can reach it.
