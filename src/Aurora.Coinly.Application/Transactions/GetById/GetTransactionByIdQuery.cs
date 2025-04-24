namespace Aurora.Coinly.Application.Transactions.GetById;

public sealed record GetTransactionByIdQuery(Guid Id) : IQuery<TransactionModel>;