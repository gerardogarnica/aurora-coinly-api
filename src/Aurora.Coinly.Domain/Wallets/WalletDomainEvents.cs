namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletCreatedEvent(Wallet wallet) : DomainEvent
{
    public Wallet Wallet { get; init; } = wallet;
}

public sealed class WalletBalanceUpdatedEvent(Wallet wallet) : DomainEvent
{
    public Wallet Wallet { get; init; } = wallet;
}