---
paths:
  - "src/Aurora.Coinly.Application/**/*.cs"
  - "src/Aurora.Coinly.Infrastructure/Database/**/*.cs"
---

# EF Core queries, ownership and tracking

There is no repository pattern; handlers query `ICoinlyDbContext` directly with LINQ, so the shape of the query is the handler's responsibility. `ICoinlyDbContext` (scoped, one context per request) is the data-access boundary.

## Every query is scoped to the current user

Coinly is single-tenant per user: wallets, categories, methods, transactions, budgets and summaries all carry `UserId`.

- Every handler that reads or mutates user data filters by `x.UserId == userContext.UserId` in the same predicate that selects the row: `SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, ...)`.
- No match → `{Entity}Errors.NotFound` (404). A row that exists but belongs to someone else is indistinguishable from one that doesn't exist; never return a "forbidden" error that would confirm it.
- An id that arrives in a command and points at *another* aggregate (`CategoryId`, `PaymentMethodId`, `WalletId` in a transaction) is loaded with the same filter before it's used, so a user can't attach another user's category.
- Domain event handlers run in the outbox job with no `IUserContext`: the user id comes from the event payload (`WalletSavingsUpdatedEvent.UserId`), and the command they dispatch carries it explicitly (`UpdateSummarySavingsCommand.UserId`).

## Parallel queries

`ICoinlyDbContext` is a single `DbContext` and is not thread-safe. A handler that runs several queries concurrently (`Task.WhenAll`) creates one context per query with `ICoinlyDbContextFactory.CreateDbContextAsync` and disposes it (`await using`), as `GetDashboardSummaryQueryHandler` does. Don't start two queries on the injected `ICoinlyDbContext` at once — EF throws "A second operation was started on this context instance".

## Query shape

- **Never `Include` or project more than one collection per query.** Sibling collections become same-level `LEFT JOIN`s and the row count is their *product*, not their sum. Split them into separate queries, or use `AsSplitQuery()` when several collections genuinely must load together.
- **Aggregate in SQL.** Totals and counts are computed inside the query (`GroupBy` + `Sum`, `Count(...)` inside a `Select`), never by loading entities and summing in memory. `Take`/`Skip` belong before `ToListAsync`, not after.
- **Lists that grow without bound are projected, not materialized.** Loading full entities and mapping with `ToModel()` is the existing style and is fine for a user's own wallets, categories or methods. Transactions and wallet history grow with every operation: project only the columns the model needs, and filter by a date range.
- **Query handlers use `AsNoTracking()`.** Some existing get-by-id handlers (`GetWalletByIdQueryHandler`) load tracked; that's wasted work but harmless. New ones don't.
- **Expression trees cannot contain `switch` expressions (CS8514)** or collection expressions (CS9175). Inside an `IQueryable` projection, use a ternary chain and `Array.Empty<T>()`.

## Raw SQL

Raw SQL is used in Infrastructure (`ProcessOutboxJob`) and is acceptable in handlers when LINQ can't express the query. Always schema-qualify (`coinly.`, via `ApplicationDbContext.DEFAULT_SCHEMA` in Infrastructure) and pass values as parameters (`NpgsqlParameter`, or the interpolated overloads `FromSql`/`ExecuteSql`, which parameterize). Never interpolate a value into a string passed to `FromSqlRaw`/`SqlQueryRaw`/`ExecuteSqlRaw`. Column names in raw SQL are snake_case.

## Tracking

- **Command handlers load the aggregate they mutate without `AsNoTracking()`**, mutate it through its domain methods and call `SaveChangesAsync`.
- **Don't call `dbContext.X.Update(entity)` on an entity loaded from the context.** It is already tracked, so `Update()` is redundant, and it marks every entity in the graph whose key is set as `Modified`. Several existing handlers still call `Update()` (`UpdateCategoryCommandHandler`, `UpdateBudgetLimitCommandHandler`...); leave them unless you're touching that handler anyway.
- **Guid keys are assigned by the domain, never by EF.** `ApplicationDbContext.OnModelCreating` sets every Guid primary key to `ValueGenerated.Never`, so a new child appended to a tracked aggregate (`user.IssueToken`, `wallet` history entries, budget periods and transactions) is tracked as `Added` and inserted on save, with no explicit `Add`. The flip side: every entity must assign its own key (`Guid.NewGuid()` in the constructor or factory, as `BaseEntity` subclasses, `WalletHistory`, `BudgetPeriod`, `BudgetTransaction` and `UserToken` do). A new entity that leaves its key unset is inserted with `Guid.Empty`, and the second one fails with a primary key violation. Existing explicit adds (`dbContext.BudgetTransactions.Add(result.Value)` in `AddTransactionToBudgetCommandHandler`) are now redundant but harmless.
- **Optimistic concurrency uses PostgreSQL's `xmin`.** `HasXminConcurrencyToken()` (`Infrastructure/Database/ConcurrencyTokenExtensions.cs`) maps a shadow `uint Version` property to the `xmin` system column, so it adds no column to the migration. `User` and `UserToken` have it. When the row changed between the read and the write, `SaveChangesAsync` throws `DbUpdateConcurrencyException` and saves nothing; the handler catches it and returns a `Result` (Refresh → `AuthenticationErrors.InvalidRefreshToken`, 401; ChangePassword → `AuthenticationErrors.ConcurrentSessionChange`, 409). There's no shared behavior that maps it, so an entity that gets a concurrency token needs that `catch` in every handler that saves it, or the conflict surfaces as a 500.
- **Any entity assigned to a navigation property on a tracked aggregate must itself be tracked.** Loading it with `AsNoTracking()` makes EF treat it as `Added` and try to insert it again (duplicate key). Entities read only for their `Id` can stay `AsNoTracking()`.

## Pagination

There is no shared pagination envelope yet: list endpoints return the whole filtered set, and `GetTransactions` is bounded by a date range. When a list needs paging, introduce it deliberately (a `spec`, a shared `PagedModel<T>` and its validator rules) rather than adding `Skip`/`Take` parameters ad hoc to one handler. Order by a unique tie-breaker (the entity `Id`) whenever paging, or pages overlap.
