using Aurora.Coinly.Application.Authentication;
using Aurora.Coinly.Application.Authentication.Login;

namespace Aurora.Coinly.Api.Endpoints.Authentication;

public sealed class Login : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "auth/login",
            async (
                [FromBody] LoginRequest request,
                ICommandHandler<LoginCommand, IdentityToken> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new LoginCommand(request.Email, request.Password);

                Result<IdentityToken> result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("Login")
            .WithTags(EndpointTags.Authentication)
            .Produces<IdentityToken>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();
    }

    internal sealed record LoginRequest(
        string Email,
        string Password);
}