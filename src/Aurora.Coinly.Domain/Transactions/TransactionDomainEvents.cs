namespace Aurora.Coinly.Domain.Transactions;

public sealed class TransactionPaidEvent(Transaction transaction) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
}

public sealed class TransactionUnpaidEvent(Transaction transaction, DateOnly paymentDate) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
    public DateOnly PaymentDate { get; init; } = paymentDate;
}