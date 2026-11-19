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

CI (`.github/workflows/build.yml`) runs restore → build (Release) → test on pushes to `main`/`staging`, then builds the Docker image to GHCR and triggers a Dokploy deploy webhook.

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
- Value objects in `Domain/Shared`: `Money`, `Currency`, `Color`, `DateRange`.
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

## Tests

- `Aurora.Coinly.Domain.UnitTests` — xUnit + FluentAssertions, organized per aggregate.
- `Aurora.Coinly.ArchitectureTests` — xUnit + NetArchTest + Shouldly.
- There are no integration tests yet, though `Program` is exposed as `public partial class` for future `WebApplicationFactory` use.
