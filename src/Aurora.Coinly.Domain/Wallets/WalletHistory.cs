namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletHistory
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public WalletHistoryType Type { get; private set; }
    public string Description { get; private set; }
    public Money Amount { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money SavingsBalance { get; private set; }
    public DateOnly Date { get; private set; }
    public bool IsIncrement => Type is WalletHistoryType.Created or WalletHistoryType.Deposit or WalletHistoryType.AssignedToAvailable;
    public Wallet Wallet { get; init; } = null!;

    private WalletHistory()
    {
        Id = Guid.NewGuid();
    }

    internal static WalletHistory Create(
        Wallet wallet,
        WalletHistoryType type,
        string description,
        Money amount,
        Money availableBalance,
        Money savingsBalance,
        DateOnly date)
    {
        return new WalletHistory
        {
            WalletId = wallet.Id,
            Type = type,
            Description = description,
            Amount = amount,
            AvailableBalance = availableBalance,
            SavingsBalance = savingsBalance,
            Date = date
        };
    }
}