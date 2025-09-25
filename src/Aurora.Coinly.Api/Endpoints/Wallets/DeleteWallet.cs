using Aurora.Coinly.Application.Wallets.Delete;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class DeleteWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "wallets/{id}",
            async (
                Guid id,
                ICommandHandler<DeleteWalletCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteWalletCommand(id);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("DeleteWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}