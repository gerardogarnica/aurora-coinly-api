using Aurora.Coinly.Application.Authentication;
using Aurora.Coinly.Application.Authentication.Refresh;

namespace Aurora.Coinly.Api.Endpoints.Authentication;

public sealed class Refresh : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "auth/refresh",
            async ([FromBody] RefreshTokenRequest request, ISender sender) =>
            {
                var command = new RefreshTokenCommand(request.RefreshToken);

                Result<IdentityToken> result = await sender.Send(command);

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