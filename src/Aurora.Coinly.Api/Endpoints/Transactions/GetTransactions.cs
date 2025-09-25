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
                [AsParameters] GetTransactionsQueryParameters queryParams,
                IQueryHandler<GetTransactionListQuery, IReadOnlyCollection<TransactionModel>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetTransactionListQuery(
                    queryParams.DateFrom,
                    queryParams.DateTo,
                    queryParams.Status,
                    queryParams.CategoryId,
                    queryParams.PaymentMethodId,
                    queryParams.DateFilterType == TransactionDateFilterType.Transaction
                        ? DisplayDateType.TransactionDate
                        : DisplayDateType.PaymentDate);

                Result<IReadOnlyCollection<TransactionModel>> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
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

        [FromQuery(Name = "dt")]
        public TransactionDateFilterType DateFilterType { get; set; } = TransactionDateFilterType.Transaction;
    }

    internal enum TransactionDateFilterType
    {
        Transaction = 0,
        Payment = 1
    }
}