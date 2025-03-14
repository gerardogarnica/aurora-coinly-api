using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetListByDateRange;

internal sealed class GetTransactionListByDateRangeQueryHandler(
    ITransactionRepository transactionRepository) : IQueryHandler<GetTransactionListByDateRangeQuery, IReadOnlyCollection<TransactionModel>>
{
    public async Task<Result<IReadOnlyCollection<TransactionModel>>> Handle(
        GetTransactionListByDateRangeQuery request, 
        CancellationToken cancellationToken)
    {
        // Get transactions
        var transactions = await transactionRepository.GetListAsync(
            DateRange.Create(request.DateFrom, request.DateTo),
            request.CategoryId,
            request.PaymentMethodId);

        // Return transaction models
        return transactions.Select(x => x.ToModel()).ToList();
    }
}