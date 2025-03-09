namespace Aurora.Coinly.Domain.Transactions;

public sealed class TransactionCreatedEvent(Transaction transaction) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
}

public sealed class TransactionPaidEvent(Transaction transaction) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
}

public sealed class TransactionRemovedEvent(Transaction transaction) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
}

public sealed class TransactionPaymentUndoneEvent(Transaction transaction) : DomainEvent
{
    public Transaction Transaction { get; init; } = transaction;
}