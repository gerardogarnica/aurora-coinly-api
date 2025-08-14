using Aurora.Coinly.Application.Wallets.AssignToAvailable;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class AssignToAvailableWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "wallets/{id}/assign/available",
            async (Guid id, [FromBody] AssignToAvailableWalletRequest request, ISender sender) =>
            {
                var command = new AssignToAvailableCommand(id, request.Amount, request.AssignedOn);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("AssignToAvailableWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record AssignToAvailableWalletRequest(decimal Amount, DateOnly AssignedOn);
}