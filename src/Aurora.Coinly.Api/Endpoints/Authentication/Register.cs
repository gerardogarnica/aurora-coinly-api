using Aurora.Coinly.Application.Authentication.Register;
using Aurora.Coinly.Application.Users;

namespace Aurora.Coinly.Api.Endpoints.Authentication;

public sealed class Register : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "auth/register",
            async (
                [FromBody] RegisterUserRequest request,
                ICommandHandler<RegisterUserCommand, UserModel> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Password);

                Result<UserModel> result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("Register")
            .WithTags(EndpointTags.Authentication)
            .Produces<UserModel>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();
    }

    internal sealed record RegisterUserRequest(
        string Email,
        string FirstName,
        string LastName,
        string Password);
}