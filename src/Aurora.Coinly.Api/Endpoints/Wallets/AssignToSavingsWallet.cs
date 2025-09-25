using Aurora.Coinly.Application.Wallets.AssignToSavings;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class AssignToSavingsWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "wallets/{id}/assign/savings",
            async (
                Guid id,
                [FromBody] AssignToSavingsWalletRequest request,
                ICommandHandler<AssignToSavingsCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new AssignToSavingsCommand(id, request.Amount, request.AssignedOn);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("AssignToSavingsWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record AssignToSavingsWalletRequest(decimal Amount, DateOnly AssignedOn);
}