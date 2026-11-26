# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Aurora Coinly is a personal finance REST API (.NET 10 / C# 14, EF Core + PostgreSQL) built with Clean Architecture: `Domain` ← `Application` ← `Infrastructure` ← `Api`. All endpoints are mounted under the `aurora/coinly/` route prefix; Swagger is at `/swagger`.

## Commands

The solution file is `Aurora Coinly.slnx` (quote it — the name has a space).

```bash
dotnet restore "Aurora Coinly.slnx"
dotnet build "Aurora Coinly.slnx"
dotnet test "Aurora Coinly.slnx"

# Single test project / single test
dotnet test test/Aurora.Coinly.Domain.UnitTests
dotnet test test/Aurora.Coinly.ArchitectureTests --filter "FullyQualifiedName~ApplicationLayerTests.Command_Should_BeSealed"

# Run the API (needs PostgreSQL; connection string "AuroraCoinlyConnection")
dotnet run --project src/Aurora.Coinly.Api

# Full stack: API (5000/5001), PostgreSQL (5432), Seq (UI 8080, OTLP ingest 5341)
docker compose up -d

# EF Core migrations
dotnet ef migrations add <Name> --project src/Aurora.Coinly.Infrastructure --startup-project src/Aurora.Coinly.Api --output-dir Database/Migrations
dotnet ef database update --project src/Aurora.Coinly.Infrastructure --startup-project src/Aurora.Coinly.Api
```

Migrations are applied automatically at startup in Development and Staging (`ApplyMigrationsAsync`).

CI (`.github/workflows/build.yml`) runs restore → build (Release) → test on pushes to `main`/`staging`, then builds the Docker image to GHCR and triggers a Dokploy deploy webhook. Pull requests to `main`/`staging` only build and test. A `vX.Y.Z` tag (matching `<Version>` in `Directory.Build.props`) re-tags the already-built image without rebuilding; see `.claude/rules/build-and-packaging.md`.

## Build strictness

`Directory.Build.props` sets `TreatWarningsAsErrors`, `AnalysisMode=All`, `EnforceCodeStyleInBuild`, and adds SonarAnalyzer to every project. Many `.editorconfig` style rules are severity `error`. **Any analyzer or style warning breaks the build** — expect to fix style issues (e.g. namespace must match folder, explicit accessibility modifiers, no `this.` qualification) rather than ignore them. Suppress Sonar rules only locally with `#pragma warning disable` when justified (see `ICommand.cs`).

Package versions are centrally managed in `Directory.Packages.props` — add `<PackageReference>` without a version in the `.csproj` and put the version there.

## Architecture

### Request flow (vertical slices, no MediatR)

Each use case lives in its own folder: `src/Aurora.Coinly.Application/<Feature>/<UseCase>/` containing `XxxCommand`/`XxxQuery`, `XxxCommandHandler`/`XxxQueryHandler`, and optionally `XxxCommandValidator`. The matching endpoint is one class per file in `src/Aurora.Coinly.Api/Endpoints/<Feature>/`.

- Custom messaging abstractions in `Application/Abstractions/Messaging`: `ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>`, `ICommandHandler<>`, `ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventHandler<>`.
- Handlers are auto-registered via Scrutor assembly scanning (`Application/DependencyInjection.cs`) and decorated with `ValidationBehavior` (commands only — runs FluentValidation validators) and `LoggingBehavior`.
- Endpoints implement `IBaseEndpoint.MapEndpoint`, are discovered by reflection (`BaseEndpointExtensions`), and inject the handler interface directly (e.g. `ICommandHandler<CreateWalletCommand, Guid>`). Endpoints map an API request record → command, call `handler.Handle`, and convert with `result.Match(onSuccess, ApiResponses.Problem)`.
- Endpoint tag names live in `Endpoints/EndpointTags.cs`. Most endpoints call `.RequireAuthorization()`; the current user comes from `IUserContext.UserId`.

### Results and errors

Handlers return `Result` / `Result<T>` (Domain `Abstractions/Result.cs`) instead of throwing. Errors are `BaseError` instances with a `BaseErrorType` (Failure/Validation/NotFound/Conflict), defined as static fields in per-aggregate `XxxErrors` classes (e.g. `WalletErrors`). `ApiResponses.Problem` maps the error type to an HTTP status and ProblemDetails; validation failures surface as a `ValidationError` with an `errors` extension.

### Domain model

- Entities inherit `BaseEntity` (Guid `Id`, domain event list), are `sealed`, have a private parameterless ctor (for EF), and are constructed only through a `public static Create(...)` factory. Behavior lives on the entity and returns `Result`.
- Value objects are `sealed record`s that implement the marker `IValueObject`: `Money`, `Currency`, `Color`, `DateRange` in `Domain/Shared`, and `Email` and `Password` in `Domain/Users`.
- Domain events derive from `DomainEvent`, are `sealed`, and end with `Event` (e.g. `WalletSavingsUpdatedEvent`, defined in `XxxDomainEvents.cs`).
- `BudgetPeriodService` is a domain service registered in Application DI.

### Domain events via Outbox

Domain events are **not** dispatched in-process on save. Flow:
1. Entity raises an event via `AddDomainEvent`.
2. `InsertOutboxMessagesInterceptor` (EF `SaveChangesInterceptor`) serializes events (Newtonsoft + `SerializerSettings.Instance`, type-name handling) into the `outbox_messages` table in the same transaction.
3. Quartz `ProcessOutboxJob` polls (`Outbox:IntervalInSeconds`, `Outbox:BatchSize`), deserializes, and dispatches through `DomainEventsDispatcher` to all `IDomainEventHandler<T>` implementations, each in its own scope.

Event handlers typically delegate to another command handler (e.g. `UpdateSummaryOnTransactionPaidEventHandler` → `AddSummaryTransactionCommand`), which is how side effects like `MonthlySummary` and budget updates happen. Changing an event's shape affects already-persisted outbox rows.

### Persistence

- `ApplicationDbContext` uses schema `coinly`, snake_case naming (`UseSnakeCaseNamingConvention`), and applies all `IEntityTypeConfiguration` classes from `Infrastructure/Configurations`.
- Application code depends on `ICoinlyDbContext` (scoped, one context per request). For handlers that need parallel queries, use `ICoinlyDbContextFactory.CreateDbContextAsync` to get independent contexts (see `GetDashboardSummaryQueryHandler`).
- Time comes from `IDateTimeService` (`UtcNow`, `Today`) — don't use `DateTime.UtcNow` directly in handlers.

### Configuration

Key sections: `ConnectionStrings:AuroraCoinlyConnection`, `Jwt` (`JwtAuthOptions`), `Encryption` (`EncryptionOptions`), `Outbox`, `Cors:AllowedOrigins`. Secrets are expected from user secrets / environment variables (docker-compose mounts the user-secrets folder).

## Conventions enforced by architecture tests

`test/Aurora.Coinly.ArchitectureTests` (NetArchTest) fails if these are violated, which fails CI:

- Layer dependencies: Domain → nothing; Application ↛ Infrastructure/Api; Infrastructure ↛ Api.
- Commands/queries: `sealed`, names end with `Command`/`Query`.
- Command/query handlers and validators: `sealed`, **not public** (use `internal`), suffixes `CommandHandler`/`QueryHandler`/`Validator`.
- Domain event handlers: `sealed`, suffix `EventHandler`.
- Endpoints: `public sealed`.
- Entities: `sealed`, private parameterless ctor, public static `Create`. Domain events: `sealed`, suffix `Event`.
- Value objects (`IValueObject`): `sealed record`, no public ctor (except the positional `Money`). Every public record in Domain outside `Abstractions` must implement `IValueObject`.

## Tests

- `Aurora.Coinly.Domain.UnitTests` — xUnit + FluentAssertions, organized per aggregate.
- `Aurora.Coinly.ArchitectureTests` — xUnit + NetArchTest + Shouldly.
- There are no integration tests yet, though `Program` is exposed as `public partial class` for future `WebApplicationFactory` use.
- Both test projects are xUnit v2, so `dotnet test` is the runner. Migrating to xUnit v3 (which breaks `dotnet test` on .NET 10 and needs the built `.exe` instead) is planned as separate work; it changes CI, the `dotnet-test-runner` agent and this file together.

## Area-specific rules

Detailed conventions live in `.claude/rules/` and load automatically when you read files matching their `paths:`. When creating something in an area without reading its existing files first, read the relevant rule.

| Rule | Covers |
|---|---|
| `ef-core-queries.md` | `UserId` ownership filter, parallel queries with the factory, query shape, raw SQL, tracking and `Update()`, pagination |
| `domain-model.md` | Enum placement, value objects, `Money`, soft delete, wallet balances and history, event payloads |
| `domain-events-outbox.md` | Outbox flow, eventual consistency, single-attempt delivery and recovery, idempotent handlers, serialization, job debt |
| `startup-and-migrations.md` | Migrations on startup (Dev/Staging only), configuration sections and secrets, pipeline order, Quartz |
| `health-and-observability.md` | `/health`, error responses, Swagger, OpenTelemetry/Seq |
| `unit-tests.md` | Domain test layout, fixed dates, assertions, naming; no Application tests yet |
| `architecture-tests.md` | NetArchTest pitfalls (`.Or()`, records, zero-type selectors), what the selectors cover |
| `build-and-packaging.md` | No spaces in project names, central package management, warnings as errors, CI |
| `documentation.md` | `docs/` layout, spec/plan/ADR naming, header and status lifecycle, Spanish for new documents |

When a change introduces a new area-specific decision, add it to the matching rule (or a new one), not here. This file only changes when the architecture, stack or workflow changes.

## Workflow

1. Ask clarifying questions if requirements are unclear.
2. Propose a plan and list files to change.
3. Implement the smallest viable change.
4. Add or update tests when appropriate.
5. Verify with the `dotnet-test-runner` agent.

Scale the process to the change:
- **Bounded** (an existing flow: a handler, an entity, an endpoint): short design in chat, approval, then implement with `domain-entity` / `create-feature` / `unit-testing`.
- **Architectural** (new feature, several aggregates, a new pattern or dependency, a complex migration): `spec` → `write-plan` → `execute-plan`, with documents in `docs/` (see `.claude/rules/documentation.md`).
- Before a PR: `arch-guard` and `code-reviewer` in parallel.

Agents: `dotnet-test-runner` (build + both test projects, reports only failures), `arch-guard` (layering, data ownership, migrations; read-only), `code-reviewer` (quality and test gaps).

## Working conventions

- Talk to the user in Spanish; code, commits and PRs are in English.
- Never commit, push or open a PR until asked. An approved plan authorizes one commit per task during `execute-plan`, nothing more.
- Stage files one by one (`git add <file>`), never `git add -A` / `git add .`. Local changes to `appsettings.Development.json` and `.claude/settings.local.json` never go into a commit.
- Branches are `feature/<topic>` from `origin/staging`; PRs target `staging`, and `staging` is merged into `main`.
- On Windows, run shell commands with the Bash tool, not PowerShell.
