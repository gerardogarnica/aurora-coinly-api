using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Application.Wallets.GetList;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class GetWallets : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "wallets",
            async ([FromQuery(Name = "deleted")] bool showDeleted, ISender sender) =>
            {
                var query = new GetWalletListQuery(showDeleted);

                Result<IReadOnlyCollection<WalletModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetWallets")
            .WithTags(EndpointTags.Wallets)
            .Produces<IReadOnlyCollection<WalletModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}