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
            async ([AsParameters] GetTransactionsQueryParameters queryParams, ISender sender) =>
            {
                var query = new GetTransactionListQuery(
                    queryParams.DateFrom,
                    queryParams.DateTo,
                    queryParams.Status,
                    queryParams.CategoryId,
                    queryParams.PaymentMethodId);

                Result<IReadOnlyCollection<TransactionModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetTransactions")
            .WithTags(EndpointTags.Transactions)
            .Produces<IReadOnlyCollection<TransactionModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record GetTransactionsQueryParameters
    {
        [FromQuery(Name = "from")]
        public DateOnly DateFrom { get; set; }

        [FromQuery(Name = "to")]
        public DateOnly DateTo { get; set; }

        [FromQuery(Name = "st")]
        public TransactionStatus? Status { get; set; }

        [FromQuery(Name = "cid")]
        public Guid? CategoryId { get; set; }

        [FromQuery(Name = "mid")]
        public Guid? PaymentMethodId { get; set; }
    }
}