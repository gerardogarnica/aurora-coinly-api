using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Application.Wallets.GetList;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class GetWallets : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "wallets",
            async (
                [FromQuery(Name = "deleted")] bool showDeleted,
                IQueryHandler<GetWalletListQuery, IReadOnlyCollection<WalletModel>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetWalletListQuery(showDeleted);

                Result<IReadOnlyCollection<WalletModel>> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetWallets")
            .WithTags(EndpointTags.Wallets)
            .Produces<IReadOnlyCollection<WalletModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}