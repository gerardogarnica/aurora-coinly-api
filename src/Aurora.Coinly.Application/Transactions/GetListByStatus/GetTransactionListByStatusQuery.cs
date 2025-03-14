using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetListByStatus;

public sealed record GetTransactionListByStatusQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    TransactionStatus Status,
    Guid? CategoryId,
    Guid? PaymentMethodId) : IQuery<IReadOnlyCollection<TransactionModel>>;