using Aurora.Coinly.Application.Transactions.UndoPayment;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class UndoTransactionPayment : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "transactions/{id}/undopayment",
            async (Guid id, ISender sender) =>
            {
                var command = new UndoTransactionPaymentCommand(id);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("UndoTransactionPayment")
            .WithTags(EndpointTags.Transactions)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}