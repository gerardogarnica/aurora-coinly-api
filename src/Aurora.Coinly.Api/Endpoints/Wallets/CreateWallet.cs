using Aurora.Coinly.Application.Wallets.Create;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Api.Endpoints.Wallets;

public sealed class CreateWallet : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "wallets",
            async ([FromBody] CreateWalletRequest request, ISender sender) =>
            {
                var command = new CreateWalletCommand(
                    request.Name,
                    request.CurrencyCode,
                    request.Amount,
                    request.Type,
                    request.AllowNegative,
                    request.Color,
                    request.Notes,
                    request.OpenedOn);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("CreateWallet")
            .WithTags(EndpointTags.Wallets)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateWalletRequest(
        string Name,
        string CurrencyCode,
        decimal Amount,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        WalletType Type,
        bool AllowNegative,
        string Color,
        string? Notes,
        DateOnly OpenedOn);
}