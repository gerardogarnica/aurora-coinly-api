using Aurora.Coinly.Application.Authentication.ChangePassword;

namespace Aurora.Coinly.Api.Endpoints.Authentication;

public sealed class ChangePassword : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "auth/change-password",
            async ([FromBody] ChangePasswordRequest request, ISender sender) =>
            {
                var command = new ChangeUserPasswordCommand(
                    request.CurrentPassword,
                    request.NewPassword);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithTags(EndpointTags.Authentication)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}