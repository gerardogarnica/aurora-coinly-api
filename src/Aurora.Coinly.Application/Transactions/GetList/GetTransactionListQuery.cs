﻿using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetList;

public sealed record GetTransactionListQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    TransactionStatus? Status,
    Guid? CategoryId,
    Guid? PaymentMethodId) : IQuery<IReadOnlyCollection<TransactionModel>>;