using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetList;

internal sealed class GetTransactionListQueryHandler(
    ITransactionRepository transactionRepository,
    IUserContext userContext) : IQueryHandler<GetTransactionListQuery, IReadOnlyCollection<TransactionModel>>
{
    public async Task<Result<IReadOnlyCollection<TransactionModel>>> Handle(
        GetTransactionListQuery request,
        CancellationToken cancellationToken)
    {
        // Get transactions
        var transactions = await transactionRepository.GetListAsync(
            userContext.UserId,
            DateRange.Create(request.DateFrom, request.DateTo),
            request.Status,
            request.CategoryId,
            request.PaymentMethodId,
            request.DisplayDateType);

        // Return transaction models
        return transactions.Select(x => x.ToModel(request.DisplayDateType)).ToList();
    }
}