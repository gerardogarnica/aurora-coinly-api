namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletBalanceUpdatedEvent(Guid walletId) : DomainEvent
{
    public Guid WalletId { get; init; } = walletId;
}

public sealed class WalletSavingsUpdatedEvent(Guid walletId, Money amount, DateOnly assignedOn, bool isIncrement) : DomainEvent
{
    public Guid WalletId { get; init; } = walletId;
    public Money Amount { get; init; } = amount;
    public DateOnly AssignedOn { get; init; } = assignedOn;
    public bool IsIncrement { get; init; } = isIncrement;
}