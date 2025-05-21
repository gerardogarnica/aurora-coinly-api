namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletBalanceUpdatedEvent(Wallet wallet) : DomainEvent
{
    public Wallet Wallet { get; init; } = wallet;
}