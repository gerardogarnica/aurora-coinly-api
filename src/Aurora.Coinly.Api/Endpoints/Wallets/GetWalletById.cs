using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Application.Wallets.GetById;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class GetWalletById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "wallets/{id}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetWalletByIdQuery(id);

                Result<WalletModel> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetWalletById")
            .WithTags(EndpointTags.Wallets)
            .Produces<WalletModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}