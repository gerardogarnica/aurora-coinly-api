---
name: create-feature
description: Implements a vertical feature slice — classifies the use case as a command or a query, writes the Application slice (command/query, handler, validator, model DTO) and the Minimal API endpoint. Use when the user asks to implement a feature or use case from a spec, add an operation like "create/update/delete/pay/transfer X", expose an endpoint or route, or fetch data with "get X by id" or "list all X". Also use when modifying an existing handler, validator, or endpoint.
argument-hint: <use case, e.g. "archive a budget" or "list transactions due this week">
---

# Create a feature

Classify the use case, write the Application slice, expose the endpoint.

The domain entity must already exist — if it does not, or a business rule has no home yet, start with the `domain-entity` skill.

## Workflow

1. **Classify.** A state change is a **command**; a read is a **query**. Name it verb + entity: `TransferBetweenWalletsCommand`, `GetBudgetListQuery`.
2. **Check the domain first.** If the operation needs a new aggregate method, a new `{Entity}Errors` entry, or a new domain event, add those before writing the handler. The rule belongs in the aggregate, never in the handler.
3. **Write the Application slice** in `src/Aurora.Coinly.Application/{Feature}/{UseCase}/`. Templates: [references/command-slice.md](references/command-slice.md) and [references/query-slice.md](references/query-slice.md).
4. **Write the endpoint** in `src/Aurora.Coinly.Api/Endpoints/{Feature}/{UseCase}.cs`. Template: [references/endpoint.md](references/endpoint.md).
5. **Write the tests.** Domain methods the slice relies on get tests in `Aurora.Coinly.Domain.UnitTests` (use the `unit-testing` skill). There is no Application test project yet, so handlers and validators are verified by the architecture tests and by hand (Swagger or `src/Aurora.Coinly.Api/Aurora.Coinly.Api.http`).
6. **Verify** — launch the `dotnet-test-runner` agent (Agent tool, `subagent_type: dotnet-test-runner`). It runs `dotnet build` (warnings are errors here) and both test projects, and reports only failures. Do not consider the feature done until that agent comes back clean — the architecture tests catch most naming, sealing and visibility mistakes in a new slice.

## Command or query

| | Command | Query |
|---|---|---|
| Interface | `ICommand` / `ICommand<T>` | `IQuery<TResponse>` |
| Handler | `ICommandHandler<TCommand>` / `<TCommand, TResponse>` | `IQueryHandler<TQuery, TResponse>` |
| Returns | `Result` / `Result<Guid>` | `Result<TResponse>` |
| Tracking | tracked load, then `SaveChangesAsync` | `AsNoTracking()` for lists; see query-slice.md |
| Validator | yes | **not run** — `ValidationBehavior` decorates commands only |

## Non-negotiable conventions

- **Folder = use case.** One folder per use case under `src/Aurora.Coinly.Application/{Feature}/` (e.g. `Wallets/Transfer/`) holding the command/query, its handler and its validator. The model DTO shared by a feature's queries lives one level up (`Wallets/WalletModel.cs`).
- **Handlers are `internal sealed`** with a primary constructor, and the method is `Handle` (not `HandleAsync`). The parameter is named `request`.
- **Handler names keep the suffix** — `CreateWalletCommandHandler`, `GetWalletByIdQueryHandler`. Validators are `{Command}Validator`: `CreateWalletCommandValidator`. The architecture tests fail otherwise.
- **Commands and queries are `public sealed record`s**; handlers, validators and endpoint request records are `internal`.
- **No manual DI registration.** Scrutor discovers handlers, validators and domain event handlers; endpoints are discovered by reflection. Never touch a `DependencyInjection.cs` for a new slice.
- **Return `Result`, never throw** for an expected failure. Errors come from `{Entity}Errors` in the Domain layer.
- **Validation lives in the validator**, not the handler. It runs automatically for commands; the handler never checks input shape.
- **Every query filters by owner.** Load with `x.UserId == userContext.UserId` (plus the id); nothing found → `{Entity}Errors.NotFound`. Never return another user's row and never answer "forbidden".
- **Data access through `ICoinlyDbContext`.** Use `ICoinlyDbContextFactory` only when a handler runs several queries in parallel (`GetDashboardSummaryQueryHandler`). Application never references Infrastructure.
- **Don't call `Update()` on a tracked entity** — it only needs `SaveChangesAsync`. See `.claude/rules/ef-core-queries.md`.
- **Queries return models, never domain entities.** `{Entity}Model` records with a `ToModel()` extension.
- **Endpoints implement `IBaseEndpoint`**, resolve the handler interface from DI, close with `result.Match(..., ApiResponses.Problem)`, tag with `EndpointTags`, and call `.RequireAuthorization()`.
- **Time comes from `IDateTimeService`** (`UtcNow`, `Today`), never `DateTime.UtcNow` in a handler.
- **`CancellationToken` is threaded through every async call.**

## The behavior pipeline

Handlers are wrapped by decorators registered with Scrutor's `Decorate(...)` in `Application/DependencyInjection.cs`. Execution order:

```
LoggingBehavior → ValidationBehavior → handler      (commands)
LoggingBehavior → handler                           (queries)
```

Logging wraps validation so the log captures the full round-trip, including validation failures. A failed validation returns a `ValidationError` (400 with an `errors` extension) without reaching the handler.

## Domain event handlers

A side effect of another aggregate (monthly summary, budget totals) is a domain event handler, not a second write in the command handler: `{Effect}On{Event}EventHandler : IDomainEventHandler<TEvent>` in the folder of the use case that raises the event (`Transactions/Process/UpdateSummaryOnTransactionPaidEventHandler.cs`). It builds a command and calls that command's handler; on failure it throws `AuroraCoinlyException` so the outbox records the error. See `.claude/rules/domain-events-outbox.md`.

## Naming

| Artifact | Pattern | Example |
|---|---|---|
| Command | `{Verb}{Entity}Command` | `CreateWalletCommand` |
| Query | `Get{X}Query` | `GetWalletByIdQuery`, `GetWalletListQuery` |
| Command handler | `{Command}Handler` | `CreateWalletCommandHandler` |
| Query handler | `{Query}Handler` | `GetWalletListQueryHandler` |
| Validator | `{Command}Validator` | `CreateWalletCommandValidator` |
| Model | `{Entity}Model` | `WalletModel` |
| Event handler | `{Effect}On{Event}EventHandler` | `UpdateSummaryOnSavingsAssignedEventHandler` |
| Endpoint | `{UseCase}.cs` in `Endpoints/{Feature}/` | `Endpoints/Wallets/CreateWallet.cs` |
| Request | nested `internal sealed record {UseCase}Request` | inside the endpoint class |
