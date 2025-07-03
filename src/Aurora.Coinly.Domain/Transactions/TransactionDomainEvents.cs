namespace Aurora.Coinly.Domain.Transactions;

public sealed class TransactionPaidEvent(Guid transactionId) : DomainEvent
{
    public Guid TransactionId { get; init; } = transactionId;
}

public sealed class TransactionUnpaidEvent(Guid transactionId, DateOnly paymentDate) : DomainEvent
{
    public Guid TransactionId { get; init; } = transactionId;
    public DateOnly PaymentDate { get; init; } = paymentDate;
}