using Aurora.Coinly.Application.Transactions;
using Aurora.Coinly.Application.Transactions.GetById;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class GetTransactionById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "transactions/{id}",
            async (
                Guid id,
                IQueryHandler<GetTransactionByIdQuery, TransactionModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetTransactionByIdQuery(id);

                Result<TransactionModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetTransactionById")
            .WithTags(EndpointTags.Transactions)
            .Produces<TransactionModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}