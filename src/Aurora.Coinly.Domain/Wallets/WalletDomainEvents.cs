namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletBalanceUpdatedEvent(Guid walletId) : DomainEvent
{
    public Guid WalletId { get; init; } = walletId;
}