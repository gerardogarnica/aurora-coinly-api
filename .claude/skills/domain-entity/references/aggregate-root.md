# Aggregate root and child entity

## Aggregate root

```csharp
// src/Aurora.Coinly.Domain/{Aggregate}/{Entity}.cs
namespace Aurora.Coinly.Domain.{Aggregate};

public sealed class {Entity} : BaseEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PRIVATE COLLECTIONS (encapsulated)
    // ═══════════════════════════════════════════════════════════════
    private readonly List<{ChildEntity}> _{childEntities} = [];

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES (private setters)
    // ═══════════════════════════════════════════════════════════════
    // NOTE: `Id` is inherited from BaseEntity — do NOT redeclare it here.
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Money TargetAmount { get; private set; }
    public Color Color { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public IReadOnlyCollection<{ChildEntity}> {ChildEntities} => _{childEntities}.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR (EF Core + Create)
    // ═══════════════════════════════════════════════════════════════
    private {Entity}() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        TargetAmount = Money.Zero();
        Color = Color.FromHex("#000000");
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════
    public static {Entity} Create(
        Guid userId,
        string name,
        Money targetAmount,
        Color color,
        string? notes,
        DateTime createdOnUtc)
    {
        var {entity} = new {Entity}
        {
            UserId = userId,
            Name = name,
            TargetAmount = targetAmount,
            Color = color,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = createdOnUtc
        };

        // AddDomainEvent is a protected INSTANCE method — call it on the new instance,
        // never bare inside this static method (that does not compile).
        {entity}.AddDomainEvent(new {Entity}CreatedEvent({entity}.Id, userId));

        return {entity};
    }

    // ═══════════════════════════════════════════════════════════════
    // DOMAIN METHODS (return Result, never throw)
    // ═══════════════════════════════════════════════════════════════
    public Result<{Entity}> Update(
        string name,
        Color color,
        string? notes,
        DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<{Entity}>({Entity}Errors.IsDeleted);
        }

        Name = name;
        Color = color;
        Notes = notes;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<{Entity}> Delete(DateTime deletedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<{Entity}>({Entity}Errors.IsDeleted);
        }

        IsDeleted = true;
        DeletedOnUtc = deletedOnUtc;

        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // CHILD ENTITY MANAGEMENT (the root owns its children)
    // ═══════════════════════════════════════════════════════════════
    public Result<{Entity}> Add{ChildEntity}(Money amount, DateOnly date, DateTime createdOnUtc)
    {
        // Invariants that need the aggregate's state are checked HERE, in the root
        if (IsDeleted)
        {
            return Result.Fail<{Entity}>({Entity}Errors.IsDeleted);
        }

        if (amount.Currency != TargetAmount.Currency)
        {
            return Result.Fail<{Entity}>({Entity}Errors.CurrenciesNotMatch);
        }

        _{childEntities}.Add({ChildEntity}.Create(Id, amount, date, createdOnUtc));
        UpdatedOnUtc = createdOnUtc;

        return this;
    }
}
```

When creation enforces invariants (a date range, an amount that must be positive), `Create` returns `Result<{Entity}>` instead, as `Transaction.Create` does:

```csharp
public static Result<{Entity}> Create(...)
{
    if (targetAmount.Amount <= 0)
    {
        return Result.Fail<{Entity}>({Entity}Errors.InvalidAmount);
    }

    var {entity} = new {Entity} { ... };

    return {entity};   // implicit conversion to Result<{Entity}>
}
```

## Child entity

A child entity does **not** inherit `BaseEntity` — it raises no domain events of its own and is never loaded independently of its root. Its factory is `internal`, so only the aggregate root can create it.

It declares its own `Id`. **Leave it unset**, as `WalletHistory` does, so EF generates it on insert and a child appended to a tracked root is inserted without help. `BudgetPeriod` and `BudgetTransaction` assign `Guid.NewGuid()` in their constructors instead. EF then treats a new instance found through a navigation as `Modified`, so every handler that adds one must call `dbContext.X.Add(child)` explicitly. Don't copy that in a new child.

```csharp
// src/Aurora.Coinly.Domain/{Aggregate}/{ChildEntity}.cs
namespace Aurora.Coinly.Domain.{Aggregate};

public sealed class {ChildEntity}
{
    public Guid Id { get; private set; }
    public Guid {Entity}Id { get; private set; }
    public Money Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }

    private {ChildEntity}()
    {
        Amount = Money.Zero();
    }

    // Takes the parent's Guid, not the parent object.
    internal static {ChildEntity} Create(
        Guid {entity}Id,
        Money amount,
        DateOnly date,
        DateTime createdOnUtc) => new()
        {
            {Entity}Id = {entity}Id,
            Amount = amount,
            Date = date,
            CreatedOnUtc = createdOnUtc
        };
}
```

`WalletHistory` is the reference child in the codebase (its `Id`/`WalletId` setters are public for historical reasons; new children keep them private).

## Anti-patterns

```csharp
// ❌ WRONG: Public setters
public string Name { get; set; }

// ✅ CORRECT: Private setters
public string Name { get; private set; }

// ❌ WRONG: Redeclaring the identifier on an aggregate root
public sealed class Wallet : BaseEntity
{
    public Guid WalletId { get; private set; }
}

// ❌ WRONG: Public constructor with all parameters
public Wallet(Guid userId, string name, Money amount, ...)

// ✅ CORRECT: Private constructor + static factory
public static Wallet Create(Guid userId, string name, Money amount, ...)

// ❌ WRONG: Throwing for an expected business failure
if (IsDeleted) throw new InvalidOperationException("Wallet is deleted");

// ✅ CORRECT: Return Result
if (IsDeleted)
{
    return Result.Fail<Wallet>(WalletErrors.IsDeleted);
}

// ❌ WRONG: Calling the instance method bare inside a static factory — does not compile
public static Wallet Create(...)
{
    var wallet = new Wallet { ... };
    AddDomainEvent(new WalletCreatedEvent(wallet.Id));
    return wallet;
}

// ✅ CORRECT: Call it on the instance
wallet.AddDomainEvent(new WalletCreatedEvent(wallet.Id));

// ❌ WRONG: Reading the clock inside the entity
UpdatedOnUtc = DateTime.UtcNow;

// ✅ CORRECT: The timestamp is a parameter
public Result<Wallet> Update(..., DateTime updatedOnUtc)

// ❌ WRONG: Exposing internal collections
public List<WalletHistory> Operations { get; set; } = [];

// ✅ CORRECT: Encapsulated collections
private readonly List<WalletHistory> _operations = [];
public IReadOnlyCollection<WalletHistory> Operations => _operations.AsReadOnly();
```
