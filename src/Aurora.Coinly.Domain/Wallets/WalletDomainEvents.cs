namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletBalanceUpdatedEvent(Guid walletId) : DomainEvent
{
    public Guid WalletId { get; init; } = walletId;
}

public sealed class WalletSavingsUpdatedEvent(Guid walletId, Money amount, bool IsIncrement) : DomainEvent
{
    public Guid WalletId { get; init; } = walletId;
    public Money Amount { get; init; } = amount;
    public bool IsIncrement { get; init; } = IsIncrement;
}