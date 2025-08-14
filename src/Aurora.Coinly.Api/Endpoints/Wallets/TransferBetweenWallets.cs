using Aurora.Coinly.Application.Wallets.Transfer;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class TransferBetweenWallets : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "wallets/transfer",
            async ([FromBody] TransferBetweenWalletsRequest request, ISender sender) =>
            {
                var command = new TransferBetweenWalletsCommand(
                    request.SourceWalletId,
                    request.DestinationWalletId,
                    request.Amount,
                    request.TransferedOn);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("TransferBetweenWallets")
            .WithTags(EndpointTags.Wallets)
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record TransferBetweenWalletsRequest(
        Guid SourceWalletId,
        Guid DestinationWalletId,
        decimal Amount,
        DateOnly TransferedOn);
}