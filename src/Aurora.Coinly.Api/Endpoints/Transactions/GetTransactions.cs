using Aurora.Coinly.Application.Transactions;
using Aurora.Coinly.Application.Transactions.GetList;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class GetTransactions : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "transactions",
            async (
                [FromQuery] DateOnly dateFrom,
                [FromQuery] DateOnly dateTo,
                [FromQuery] TransactionStatus? status,
                [FromQuery] Guid? categoryId,
                [FromQuery] Guid? paymentMethodId,
                ISender sender) =>
            {
                var query = new GetTransactionListQuery(dateFrom, dateTo, status, categoryId, paymentMethodId);

                Result<IReadOnlyCollection<TransactionModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetTransactions")
            .WithTags(EndpointTags.Transactions)
            .Produces<IReadOnlyCollection<TransactionModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}