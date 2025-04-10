using Aurora.Coinly.Application.Transactions;
using Aurora.Coinly.Application.Transactions.GetById;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class GetTransactionById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "transactions/{id}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetTransactionByIdQuery(id);

                Result<TransactionModel> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetTransactionById")
            .WithTags(EndpointTags.Transactions)
            .Produces<TransactionModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}