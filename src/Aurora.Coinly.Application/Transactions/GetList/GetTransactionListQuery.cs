namespace Aurora.Coinly.Application.Transactions.GetList;

public sealed record GetTransactionListQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    TransactionStatus? Status,
    Guid? CategoryId,
    Guid? PaymentMethodId,
    DisplayDateType DisplayDateType) : IQuery<IReadOnlyCollection<TransactionModel>>;