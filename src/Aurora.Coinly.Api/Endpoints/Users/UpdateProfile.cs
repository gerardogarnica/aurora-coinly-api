using Aurora.Coinly.Application.Users.UpdateUser;

namespace Aurora.Coinly.Api.Endpoints.Users;

public sealed class UpdateProfile : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "users/me",
            async (
                [FromBody] UpdateProfileRequest request,
                ICommandHandler<UpdateUserCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateUserCommand(
                    request.FirstName,
                    request.LastName);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .WithTags(EndpointTags.Users)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdateProfileRequest(
        string FirstName,
        string LastName);
}
