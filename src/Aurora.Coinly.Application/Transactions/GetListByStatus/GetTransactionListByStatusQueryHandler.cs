using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetListByStatus;

internal sealed class GetTransactionListByStatusQueryHandler(
    ITransactionRepository transactionRepository) : IQueryHandler<GetTransactionListByStatusQuery, IReadOnlyCollection<TransactionModel>>
{
    public async Task<Result<IReadOnlyCollection<TransactionModel>>> Handle(
        GetTransactionListByStatusQuery request,
        CancellationToken cancellationToken)
    {
        // Get transactions
        var transactions = await transactionRepository.GetListByStatusAsync(
            DateRange.Create(request.DateFrom, request.DateTo),
            request.Status,
            request.CategoryId,
            request.PaymentMethodId);

        // Return transaction models
        return transactions.Select(x => x.ToModel()).ToList();
    }
}