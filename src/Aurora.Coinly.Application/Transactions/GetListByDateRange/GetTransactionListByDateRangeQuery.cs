namespace Aurora.Coinly.Application.Transactions.GetListByDateRange;

public sealed record GetTransactionListByDateRangeQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    Guid? CategoryId,
    Guid? PaymentMethodId) : IQuery<IReadOnlyCollection<TransactionModel>>;