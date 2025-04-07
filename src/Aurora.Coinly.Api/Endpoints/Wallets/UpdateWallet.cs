using Aurora.Coinly.Application.Wallets.Update;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class UpdateWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "wallets/{id}",
            async (Guid id, [FromBody] UpdateWalletRequest request, ISender sender) =>
            {
                var command = new UpdateWalletCommand(id, request.Name, request.Notes);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .WithName("UpdateWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdateWalletRequest(string Name, string? Notes);
}