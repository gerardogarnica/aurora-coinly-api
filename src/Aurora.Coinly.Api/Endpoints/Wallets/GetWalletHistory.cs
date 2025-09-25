using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Application.Wallets.GetHistory;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class GetWalletHistory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "wallets/{id}/history",
            async (
                Guid id,
                [FromQuery] DateOnly dateFrom,
                [FromQuery] DateOnly dateTo,
                IQueryHandler<GetWalletHistoryQuery, WalletModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetWalletHistoryQuery(id, dateFrom, dateTo);

                Result<WalletModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetWalletHistory")
            .WithTags(EndpointTags.Wallets)
            .Produces<WalletModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}