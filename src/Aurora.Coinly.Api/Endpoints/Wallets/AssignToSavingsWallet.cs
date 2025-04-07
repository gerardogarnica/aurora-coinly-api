using Aurora.Coinly.Application.Wallets.AssignToSavings;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class AssignToSavingsWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "wallets/{id}/assign/savings",
            async (Guid id, [FromBody] AssignToSavingsWalletRequest request, ISender sender) =>
            {
                var command = new AssignToSavingsCommand(id, request.Amount, request.AssignedOn);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .WithName("AssignToSavingsWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record AssignToSavingsWalletRequest(decimal Amount, DateOnly AssignedOn);
}