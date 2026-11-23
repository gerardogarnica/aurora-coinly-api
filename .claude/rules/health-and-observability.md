---
paths:
  - "src/Aurora.Coinly.Api/Extensions/HealthCheckExtensions.cs"
  - "src/Aurora.Coinly.Api/DependencyInjection.cs"
  - "src/Aurora.Coinly.Api/Program.cs"
  - "src/Aurora.Coinly.Api/Middlewares/**/*.cs"
  - "src/Aurora.Coinly.Api/Dockerfile"
  - "docker-compose*.yml"
---

# Health checks, errors and observability

## Health

- `/health` is mapped by `MapHealthCheckEndpoints` (`Api/Extensions/HealthCheckExtensions.cs`) outside the `aurora/coinly/` route group, anonymous. `AddHealthCheckServices` registers no checks, so today it answers "is the process up", never "can it reach the database".
- Point the container or orchestrator health check (Dokploy) at `/health` as it is. If a database check is added (tagged `ready`, with a timeout, created from `IDbContextFactory<ApplicationDbContext>`), split the endpoints first: a liveness endpoint with no dependency checks for the orchestrator, and the readiness one for monitoring. Otherwise a database outage makes the orchestrator restart a healthy API in a loop.
- A failing check must return a fixed description, never the host, the port or the exception message — `/health` is anonymous.

## Errors

- Expected failures are `Result`s mapped by `ApiResponses.Problem` to ProblemDetails (`type`, `title`, `status`, `detail`, plus `errors` for validation).
- Unexpected exceptions go through `ValidationExceptionHandler` and `GlobalExceptionHandler` (`Api/Middlewares/`), registered with `AddExceptionHandler`; `AddProblemDetails` adds a `requestId` (the trace identifier) to every ProblemDetails. A 500 never carries the exception message.

## Swagger

`UseSwagger`/`UseSwaggerUI` run in every environment, including Production, and the document declares no security scheme, so the UI has no Authorize button. Restricting it to Development/Staging and adding the `Bearer` scheme is pending; until then, test protected endpoints with `src/Aurora.Coinly.Api/Aurora.Coinly.Api.http` or a client that sends `Authorization: Bearer <token>`.

## OpenTelemetry

- `AddObservability` (`Api/DependencyInjection.cs`) configures tracing (ASP.NET Core, HttpClient, Npgsql), metrics (ASP.NET Core, HttpClient, runtime) and logs with scopes and formatted messages, exported over OTLP (`UseOtlpExporter`). The service name is the application name.
- Locally, Docker Compose sends OTLP to Seq (`OTEL_EXPORTER_OTLP_ENDPOINT=http://aurora.coinly.seq:5341/ingest/otlp`, UI on http://localhost:8080). Without that variable the exporter targets the OTLP default endpoint.
- There is no Serilog; log through `ILogger<T>` with message templates (`"... {OutboxId}"`), never string interpolation — Sonar flags it and it defeats structured logging.
- `/health` is not excluded from tracing, so health probes show up as spans.
