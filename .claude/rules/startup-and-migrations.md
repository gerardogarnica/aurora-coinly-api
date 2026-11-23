---
paths:
  - "src/Aurora.Coinly.Api/Program.cs"
  - "src/Aurora.Coinly.Api/Extensions/**/*.cs"
  - "src/Aurora.Coinly.Api/DependencyInjection.cs"
  - "src/**/appsettings*.json"
  - "src/Aurora.Coinly.Infrastructure/DependencyInjection.cs"
  - "src/Aurora.Coinly.Infrastructure/Database/Migrations/**"
  - "docker-compose*.yml"
---

# Startup, configuration and migrations

## Migrations

- `Program.cs` calls `ApplyMigrationsAsync` (`Api/Extensions/MigrationServiceExtensions.cs`) only when the environment is Development or Staging. **Production does not migrate on startup**: a release with a migration needs it applied by hand (`dotnet ef database update`, or a generated script) before or with the deploy.
- Because Staging migrates on startup, a broken migration takes the staging deploy down on its first request. Review migrations with `arch-guard` before merging into `staging`.
- Migrations live in `Infrastructure/Database/Migrations/` (pass `--output-dir Database/Migrations`). The history table is `coinly.__EFMigrationsHistory`.
- There is a single `InitialMigration`. Don't edit an applied migration; add a new one.

## Configuration

| Section | Bound to | Notes |
|---|---|---|
| `ConnectionStrings:AuroraCoinlyConnection` | Npgsql | Docker Compose provides Postgres on 5432 (`auroracoinly` / `postgres`) |
| `Jwt` | `JwtAuthOptions` | `Key`, `Issuer`, `Audience`, `LifeTimeInMinutes`, `RefreshTokenExpirationDays` |
| `Encryption` | `EncryptionOptions` | `MasterKey` (base64) |
| `Outbox` | `OutboxOptions` | `IntervalInSeconds`, `BatchSize` |
| `Cors:AllowedOrigins` | `CorsExtensions.FrontendPolicy` | empty array in `appsettings.json` |

- None of these options is validated on start: a missing `Jwt:Key` or `Outbox:IntervalInSeconds` shows up as a failure at first use, not as a startup error. Adding `ValidateOnStart` is a welcome change when touching an options class.
- Secrets (`Jwt:Key`, `Encryption:MasterKey`, the connection string password) come from user secrets locally and environment variables in staging/production (`Jwt__Key`). `appsettings.Development.json` keeps placeholders. **Never commit real values**, and never stage local changes to `appsettings.Development.json` along with feature work.

## Pipeline order

`Program.cs` maps endpoints under the `aurora/coinly/` route group, then: Swagger → migrations (Dev/Staging) → `UseHttpsRedirection` → `UseCors(FrontendPolicy)` → `UseExceptionHandler` → `UseStatusCodePages` → `UseAuthentication` → `UseAuthorization`. A new middleware that must see authenticated requests goes after `UseAuthentication`; one whose responses need CORS headers goes after `UseCors`.

## Quartz

Quartz is registered with `WaitForJobsToComplete = true`; jobs are configured through `IConfigureOptions<QuartzOptions>` (`ConfigureProcessOutboxJob`). A new recurring job follows that pattern and reads its interval from its own options section.
