using Aurora.Coinly.Application.Authentication;
using Aurora.Coinly.Application.Authentication.Refresh;

namespace Aurora.Coinly.Api.Endpoints.Authentication;

public sealed class Refresh : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "auth/refresh",
            async (
                [FromBody] RefreshTokenRequest request,
                ICommandHandler<RefreshTokenCommand, IdentityToken> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RefreshTokenCommand(request.RefreshToken);

                Result<IdentityToken> result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("Refresh")
            .WithTags(EndpointTags.Authentication)
            .Produces<IdentityToken>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();
    }

    internal sealed record RefreshTokenRequest(string RefreshToken);
}