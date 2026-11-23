# EF Core configuration

Maps domain entities to tables with the Fluent API, keeping the Domain layer persistence-ignorant.

## Core principles

1. Domain MUST remain persistence-ignorant
2. One configuration per entity, `internal sealed class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>`
3. Fluent API only — no data annotations
4. Configurations MUST be deterministic and version-safe

## Location

```
src/Aurora.Coinly.Infrastructure/Configurations/
```

Discovered automatically by `ApplyConfigurationsFromAssembly` in `ApplicationDbContext.OnModelCreating` — no manual registration. (`OutboxMessageConfiguration` lives in `Outbox/` instead, next to its entity.)

## Global conventions already in effect

Two settings apply to every configuration, so do not restate them per entity:

- **Default schema `coinly`** — `modelBuilder.HasDefaultSchema(ApplicationDbContext.DEFAULT_SCHEMA)`. Never pass a schema to `ToTable()`.
- **snake_case naming** — `.UseSnakeCaseNamingConvention()` (`Infrastructure/DependencyInjection.cs`) rewrites every table and column name. This is why `HasColumnName` is unnecessary except on owned types, where the convention cannot infer the flattened column name.

## Naming

| Element | Convention | Example |
|--------|------------|--------|
| Configuration class | `{Entity}Configuration` | `WalletConfiguration` |
| Table name | snake_case plural, set with `ToTable` | `wallets`, `budget_periods` (`wallet_history` is a historical exception) |
| Column names | snake_case (automatic) | `created_on_utc` |

## Properties

- String properties MUST have `HasMaxLength()` matching the validator (`Name` 100, `Notes` 1000).
- Money amounts are `numeric(9, 2)`, matching `.PrecisionScale(9, 2, true)` in the validators.
- Enums are stored as strings: `.HasConversion<string>().HasMaxLength(40)`.
- Foreign keys and relationships MUST be configured explicitly.

```csharp
builder.Property(x => x.Name).HasMaxLength(100);

builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);

builder.Property(x => x.Notes).HasMaxLength(1000);
```

## Primary keys

```csharp
builder.HasKey(x => x.Id);
```

Always explicit.

## Relationships

Child collection owned by the root (configured from the child side, as `WalletHistoryConfiguration` does):

```csharp
builder
    .HasOne<Wallet>()
    .WithMany(x => x.Operations)
    .HasForeignKey(x => x.WalletId)
    .IsRequired();
```

Reference to another aggregate:

```csharp
builder
    .HasOne(x => x.Category)
    .WithMany()
    .HasForeignKey(x => x.CategoryId)
    .IsRequired();
```

## Private collection navigations

Aggregates encapsulate their children behind a private field exposed as a read-only collection:

```csharp
private readonly List<WalletHistory> _operations = [];
public IReadOnlyCollection<WalletHistory> Operations => _operations.AsReadOnly();
```

EF finds the backing field by convention when it is named `_camelCase` after the property (`_operations` ↔ `Operations`), so no `HasField` is needed. If the names don't match, declare it:

```csharp
builder.Navigation(x => x.Operations)
    .HasField("_history")
    .UsePropertyAccessMode(PropertyAccessMode.Field);
```

A private field with no public property at all is mapped by name, as `UserConfiguration` does with `_passwordHash`.

## Value objects

Configured as owned types, flattened into the parent table:

```csharp
builder.OwnsOne(x => x.AvailableAmount, y =>
{
    y.Property(x => x.Amount)
        .HasColumnName("available_amount")
        .HasColumnType("numeric(9, 2)");

    y.Property(x => x.Currency)
        .HasConversion(x => x.Code, code => Currency.FromCode(code))
        .HasMaxLength(3);
});

builder.OwnsOne(x => x.Color, y =>
{
    y.Property(x => x.Value).HasColumnName("color").HasMaxLength(7);
});
```

## Indexes

- Index `UserId` on every new aggregate root: every query filters on it.
- Index foreign keys that are frequently queried and fields used in filters (`Status`, dates).
- Add unique indexes where the domain implies uniqueness (`users.email`).

```csharp
builder.HasIndex(x => x.UserId);

builder.HasIndex(x => x.Email).IsUnique();
```

## Ignoring

```csharp
// Computed property that must not be persisted
builder.Ignore(x => x.TotalAmount);
```

## Complete example

```csharp
// src/Aurora.Coinly.Infrastructure/Configurations/{Entity}Configuration.cs
namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{entities}");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.OwnsOne(x => x.TargetAmount, y =>
        {
            y.Property(x => x.Amount)
                .HasColumnName("target_amount")
                .HasColumnType("numeric(9, 2)");

            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.Color, y =>
        {
            y.Property(x => x.Value).HasColumnName("color").HasMaxLength(7);
        });

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => x.UserId);
    }
}
```

## Critical rules

1. Domain entities MUST NOT contain EF attributes
2. No business logic inside configurations
3. All relationships MUST be defined explicitly
4. String lengths and decimal precision MUST match the validators
5. Value objects as owned types
6. Never pass a schema to `ToTable()` and never add `HasColumnName` outside owned types

## Anti-patterns

❌ Using data annotations in domain entities
❌ Hardcoding a string length that differs from the validator's `MaximumLength`
❌ Adding `HasColumnName` outside owned types (snake_case is already global)
❌ Passing a schema to `ToTable()` (the default schema is already set)
❌ Rewriting an existing enum mapping (`int` ↔ `string`) — that is a destructive migration over existing data
❌ A new aggregate root without an index on `UserId`
