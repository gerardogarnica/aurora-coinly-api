using Aurora.Coinly.Application.Users.Update;

namespace Aurora.Coinly.Api.Endpoints.Profile;

public sealed class UpdateProfile : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "me",
            async ([FromBody] UpdateProfileRequest request, ISender sender) =>
            {
                var command = new UpdateUserCommand(
                    request.FirstName,
                    request.LastName);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .WithTags(EndpointTags.Profile)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdateProfileRequest(
        string FirstName,
        string LastName);
}