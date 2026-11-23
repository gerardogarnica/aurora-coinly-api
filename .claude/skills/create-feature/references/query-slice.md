# Query slice

A query is a read with no state change. Every query slice lives in its own folder; the model it returns is shared by the feature and lives one level up:

```
src/Aurora.Coinly.Application/{Feature}/
├── {Entity}Model.cs                       ← record + ToModel() extension, shared by the feature's queries
├── GetById/
│   ├── Get{Entity}ByIdQuery.cs
│   └── Get{Entity}ByIdQueryHandler.cs
└── GetList/
    ├── Get{Entity}ListQuery.cs
    └── Get{Entity}ListQueryHandler.cs
```

**Queries are not validated.** `ValidationBehavior` decorates `ICommandHandler<>` and `ICommandHandler<,>` only (`Application/DependencyInjection.cs`), so a validator written for a query never runs. Route constraints and model binding cover the input; anything else (a date range that must be ordered) is checked in the handler and returned as a `Result.Fail(...)`, or the decorator is extended to queries as a deliberate change.

| Shape | Returns |
|---|---|
| Get by Id | `Result<{Entity}Model>` |
| Get all / filtered | `Result<IReadOnlyCollection<{Entity}Model>>` |

## The model

```csharp
// src/Aurora.Coinly.Application/{Feature}/{Entity}Model.cs
namespace Aurora.Coinly.Application.{Feature};

public sealed record {Entity}Model(
    Guid {Entity}Id,
    string Name,
    string CurrencyCode,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    {Entity}Type Type,
    string Color,
    bool IsDeleted,
    string? Notes);

internal static class {Entity}ModelExtensions
{
    internal static {Entity}Model ToModel(this {Entity} {entity}) => new(
        {entity}.Id,
        {entity}.Name,
        {entity}.Amount.Currency.Code,
        {entity}.Amount.Amount,
        {entity}.Type,
        {entity}.Color.Value,
        {entity}.IsDeleted,
        {entity}.Notes);
}
```

Models flatten value objects (`Money` → `CurrencyCode` + `Amount`, `Color` → its hex string) so the JSON contract doesn't depend on the domain types.

## Get by Id

```csharp
// src/Aurora.Coinly.Application/{Feature}/GetById/Get{Entity}ByIdQuery.cs
namespace Aurora.Coinly.Application.{Feature}.GetById;

public sealed record Get{Entity}ByIdQuery(Guid Id) : IQuery<{Entity}Model>;
```

```csharp
// src/Aurora.Coinly.Application/{Feature}/GetById/Get{Entity}ByIdQueryHandler.cs
namespace Aurora.Coinly.Application.{Feature}.GetById;

internal sealed class Get{Entity}ByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<Get{Entity}ByIdQuery, {Entity}Model>
{
    public async Task<Result<{Entity}Model>> Handle(
        Get{Entity}ByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get {entity}
        {Entity}? {entity} = await dbContext
            .{Entities}
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if ({entity} is null)
        {
            return Result.Fail<{Entity}Model>({Entity}Errors.NotFound);
        }

        // Return {entity} model
        return {entity}.ToModel();
    }
}
```

## Get all, with a filter

```csharp
// src/Aurora.Coinly.Application/{Feature}/GetList/Get{Entity}ListQuery.cs
namespace Aurora.Coinly.Application.{Feature}.GetList;

public sealed record Get{Entity}ListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<{Entity}Model>>;
```

```csharp
// src/Aurora.Coinly.Application/{Feature}/GetList/Get{Entity}ListQueryHandler.cs
namespace Aurora.Coinly.Application.{Feature}.GetList;

internal sealed class Get{Entity}ListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<Get{Entity}ListQuery, IReadOnlyCollection<{Entity}Model>>
{
    public async Task<Result<IReadOnlyCollection<{Entity}Model>>> Handle(
        Get{Entity}ListQuery request,
        CancellationToken cancellationToken)
    {
        // Get {entities}
        IQueryable<{Entity}> query = dbContext
            .{Entities}
            .Where(x => x.UserId == userContext.UserId)
            .AsNoTracking();

        if (!request.ShowDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        List<{Entity}> {entities} = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        // Return {entity} models
        return {entities}.Select(x => x.ToModel()).ToList();
    }
}
```

## Materialize-then-map vs. projection

The existing slices load entities and map them with `ToModel()`. That is fine for a user's own wallets, categories or methods (tens of rows). For anything that grows without bound (transactions, wallet history, dashboard aggregates) project in the database instead, so only the needed columns travel and counts/sums run in SQL:

```csharp
var expenses = await dbContext
    .Transactions
    .AsNoTracking()
    .Where(x => x.UserId == userContext.UserId && x.Status == TransactionStatus.Paid)
    .GroupBy(x => x.CategoryId)
    .Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.Amount.Amount) })
    .ToListAsync(cancellationToken);
```

See `.claude/rules/ef-core-queries.md` for query shape rules.

## Rules

- Queries are `public sealed record`s implementing `IQuery<TResponse>`; one handler each, `internal sealed`, suffix `QueryHandler`.
- **Return models, never domain entities.**
- **Always filter by `userContext.UserId`.**
- `AsNoTracking()` — nothing in a query is meant to be saved.
- Always thread `CancellationToken`.
- No `Result.Ok(...)` needed: `Result<T>` converts implicitly from `T`.

## Anti-patterns

```csharp
// ❌ WRONG: exposing the domain model
public sealed record Get{Entity}ByIdQuery(Guid Id) : IQuery<{Entity}>;

// ✅ CORRECT: a model DTO
public sealed record Get{Entity}ByIdQuery(Guid Id) : IQuery<{Entity}Model>;

// ❌ WRONG: a query validator — it is never executed
internal sealed class Get{Entity}ByIdQueryValidator : AbstractValidator<Get{Entity}ByIdQuery> { }

// ❌ WRONG: string interpolation into SQL
string sql = $"SELECT * FROM coinly.wallets WHERE name = '{request.Name}'";

// ✅ CORRECT: LINQ, or parameters with raw SQL
```
