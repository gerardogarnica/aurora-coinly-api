using Aurora.Coinly.Application.Transactions.Process;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class PayTransaction : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "transactions/pay",
            async ([FromBody] PayTransactionRequest request, ISender sender) =>
            {
                var command = new ProcessTransactionPaymentCommand(
                    request.TransactionIds,
                    request.WalletId,
                    request.PaymentDate);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("PayTransaction")
            .WithTags(EndpointTags.Transactions)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record PayTransactionRequest(Guid[] TransactionIds, Guid WalletId, DateOnly PaymentDate);
}