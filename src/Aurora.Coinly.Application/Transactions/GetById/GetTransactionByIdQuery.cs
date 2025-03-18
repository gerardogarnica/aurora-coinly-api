namespace Aurora.Coinly.Application.Transactions.GetById;

internal sealed record GetTransactionByIdQuery(Guid Id) : IQuery<TransactionModel>;