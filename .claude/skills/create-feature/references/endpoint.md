# Minimal API endpoint

One file per use case under `src/Aurora.Coinly.Api/Endpoints/{Feature}/`. The class implements `IBaseEndpoint` and is discovered by reflection (`BaseEndpointExtensions.AddEndpoints`) — never register it by hand. It must be `public sealed` (`ApiLayerTests`).

The endpoint binds the request, dispatches, and maps the `Result`. Nothing else.

## Naming and verbs

| Operation | Class | Verb | Success |
|---|---|---|---|
| Create | `Create{Entity}` | POST | 201 Created |
| Update | `Update{Entity}` | PUT | 202 Accepted |
| Delete (soft) | `Delete{Entity}` | DELETE | 202 Accepted |
| State change | `{Verb}{Entity}` (`PayTransaction`, `SetDefaultPaymentMethod`) | PUT on its own segment | 202 Accepted |
| Get by id | `Get{Entity}ById` | GET | 200 OK |
| Get all | `Get{Entities}` | GET | 200 OK |

Routes are lowercase plural (`wallets/{id}`, `wallets/{id}/assign/savings`, `methods/{id}/default`). The endpoint declares only the resource segment; the `aurora/coinly/` prefix is applied centrally by the route group in `Program.cs`.

## Create — POST

```csharp
// src/Aurora.Coinly.Api/Endpoints/{Feature}/Create{Entity}.cs
using Aurora.Coinly.Application.{Feature}.Create;
using Aurora.Coinly.Domain.{Feature};

namespace Aurora.Coinly.Api.Endpoints.{Feature};

public sealed class Create{Entity} : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "{feature}",
            async (
                [FromBody] Create{Entity}Request request,
                ICommandHandler<Create{Entity}Command, Guid> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new Create{Entity}Command(
                    request.Name,
                    request.CurrencyCode,
                    request.Amount,
                    request.Color,
                    request.Notes);

                Result<Guid> result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("Create{Entity}")
            .WithTags(EndpointTags.{Feature})
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record Create{Entity}Request(
        string Name,
        string CurrencyCode,
        decimal Amount,
        string Color,
        string? Notes);
}
```

The request record is `internal sealed` and **nested inside the endpoint class** — it belongs to this route and nothing else should reference it. An enum property gets `[property: JsonConverter(typeof(JsonStringEnumConverter))]` (see `CreateWallet.CreateWalletRequest`).

## Update — PUT

```csharp
// src/Aurora.Coinly.Api/Endpoints/{Feature}/Update{Entity}.cs
using Aurora.Coinly.Application.{Feature}.Update;

namespace Aurora.Coinly.Api.Endpoints.{Feature};

public sealed class Update{Entity} : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "{feature}/{id}",
            async (
                Guid id,
                [FromBody] Update{Entity}Request request,
                ICommandHandler<Update{Entity}Command> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new Update{Entity}Command(
                    id,
                    request.Name,
                    request.Color,
                    request.Notes);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("Update{Entity}")
            .WithTags(EndpointTags.{Feature})
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record Update{Entity}Request(
        string Name,
        string Color,
        string? Notes);
}
```

The id comes from the route, never from the body — that removes any route/body mismatch.

## Delete — DELETE

Deletes are soft: the command calls `{entity}.Delete(dateTimeService.UtcNow)`, which sets `IsDeleted`/`DeletedOnUtc`.

```csharp
public sealed class Delete{Entity} : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "{feature}/{id}",
            async (
                Guid id,
                ICommandHandler<Delete{Entity}Command> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new Delete{Entity}Command(id);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("Delete{Entity}")
            .WithTags(EndpointTags.{Feature})
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

## Get by id — GET

```csharp
using Aurora.Coinly.Application.{Feature};
using Aurora.Coinly.Application.{Feature}.GetById;

namespace Aurora.Coinly.Api.Endpoints.{Feature};

public sealed class Get{Entity}ById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "{feature}/{id}",
            async (
                Guid id,
                IQueryHandler<Get{Entity}ByIdQuery, {Entity}Model> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Get{Entity}ByIdQuery(id);

                Result<{Entity}Model> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("Get{Entity}ById")
            .WithTags(EndpointTags.{Feature})
            .Produces<{Entity}Model>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

For a `Result<T>` the `Match(Results.Ok, ApiResponses.Problem)` overload passes the value through directly (`Responses/ResultExtensions.cs`).

## Get all — GET

```csharp
public sealed class Get{Entities} : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "{feature}",
            async (
                [FromQuery(Name = "deleted")] bool showDeleted,
                IQueryHandler<Get{Entity}ListQuery, IReadOnlyCollection<{Entity}Model>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Get{Entity}ListQuery(showDeleted);

                Result<IReadOnlyCollection<{Entity}Model>> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("Get{Entities}")
            .WithTags(EndpointTags.{Feature})
            .Produces<IReadOnlyCollection<{Entity}Model>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

## Authorization

Every endpoint calls `.RequireAuthorization()`. Only `auth/login` and `auth/refresh` call `.AllowAnonymous()`. There are no role policies: authorization is ownership, enforced in the handler by filtering on `userContext.UserId`.

## Rules

1. Never inject `DbContext` or a domain service into an endpoint — only the handler interface.
2. No business logic and no branching on business state.
3. Never return a domain entity.
4. Always pass `CancellationToken` to `handler.Handle(...)`.
5. No manual validation — the validator runs in the pipeline.
6. Declare the status codes the route can produce so the OpenAPI document matches reality: the success code with `.Produces(...)`, each error the handler can return with `.Produces<ProblemDetails>(...)` (`Validation` → 400, `NotFound` → 404, `Conflict` → 409, see `ApiResponses.GetStatusCodeFromError`), and always 500.
7. A new feature gets its tag constant in `Endpoints/EndpointTags.cs`.
