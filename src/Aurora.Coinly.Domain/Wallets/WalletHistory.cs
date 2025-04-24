namespace Aurora.Coinly.Domain.Wallets;

public sealed class WalletHistory
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid? TransactionId { get; set; }
    public WalletHistoryType Type { get; private set; }
    public string Description { get; private set; }
    public DateOnly Date { get; private set; }
    public Money Amount { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money SavingsBalance { get; private set; }
    public bool IsIncrement => Type is WalletHistoryType.Created or WalletHistoryType.Deposit or WalletHistoryType.AssignedToAvailable;
    public DateTime CreatedOnUtc { get; private set; }

    private WalletHistory() { }

    internal static WalletHistory Create(
        Guid walletId,
        Guid? transactionId,
        WalletHistoryType type,
        string description,
        DateOnly date,
        Money amount,
        Money availableBalance,
        Money savingsBalance,
        DateTime createdOn)
    {
        return new WalletHistory
        {
            WalletId = walletId,
            TransactionId = transactionId,
            Type = type,
            Description = description,
            Date = date,
            Amount = amount,
            AvailableBalance = availableBalance,
            SavingsBalance = savingsBalance,
            CreatedOnUtc = createdOn
        };
    }
}