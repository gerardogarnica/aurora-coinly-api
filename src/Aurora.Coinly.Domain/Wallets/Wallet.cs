namespace Aurora.Coinly.Domain.Wallets;

public sealed class Wallet : BaseEntity
{
    private readonly List<WalletHistory> _walletHistory = [];

    public string Name { get; private set; }
    public Money AvailableAmount { get; private set; }
    public Money SavingsAmount { get; private set; }
    public Money TotalAmount => AvailableAmount + SavingsAmount;
    public WalletType Type { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public IReadOnlyCollection<WalletHistory> Operations => _walletHistory.AsReadOnly();

    private Wallet() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        AvailableAmount = Money.Zero();
        SavingsAmount = Money.Zero();
        Type = WalletType.Cash;
    }

    public static Wallet Create(
        string name,
        Money amount,
        WalletType type,
        string? notes,
        DateTime createdOn)
    {
        var wallet = new Wallet
        {
            Name = name,
            AvailableAmount = amount,
            SavingsAmount = Money.Zero(amount.Currency),
            Type = type,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = createdOn
        };

        wallet.AddOperation(
            WalletHistoryType.Created,
            "Wallet created",
            amount,
            DateOnly.FromDateTime(createdOn));

        wallet.AddDomainEvent(new WalletCreatedEvent(wallet));

        return wallet;
    }

    public Result<Wallet> Update(string name, string? notes, DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<Wallet>(WalletErrors.IsDeleted);
        }

        Name = name;
        Notes = notes;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<Wallet> AssignToSavings(Money amount, DateOnly assignedOn, DateTime updatedOnUtc)
    {
        if (AvailableAmount < amount)
        {
            return Result.Fail<Wallet>(WalletErrors.UnableToAssignToSavings);
        }

        AvailableAmount -= amount;
        SavingsAmount += amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.AssignedToSavings,
            "Assigned to savings",
            amount,
            assignedOn);

        AddDomainEvent(new WalletBalanceUpdatedEvent(this));

        return this;
    }

    public Result<Wallet> AssignToAvailable(Money amount, DateOnly assignedOn, DateTime updatedOnUtc)
    {
        if (SavingsAmount < amount)
        {
            return Result.Fail<Wallet>(WalletErrors.UnableToAssignToAvailable);
        }

        SavingsAmount -= amount;
        AvailableAmount += amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.AssignedToAvailable,
            "Assigned to available",
            amount,
            assignedOn);

        AddDomainEvent(new WalletBalanceUpdatedEvent(this));

        return this;
    }

    public Result<Wallet> Deposit(Money amount, string description, DateOnly processedOn, DateTime updatedOnUtc)
    {
        AvailableAmount += amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.Deposit,
            description,
            amount,
            processedOn);

        AddDomainEvent(new WalletBalanceUpdatedEvent(this));

        return this;
    }

    public Result<Wallet> Withdraw(Money amount, string description, DateOnly processedOn, DateTime updatedOnUtc)
    {
        AvailableAmount -= amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.Withdrawal,
            description,
            amount,
            processedOn);

        AddDomainEvent(new WalletBalanceUpdatedEvent(this));

        return this;
    }

    public Result<Wallet> Delete(DateTime deletedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<Wallet>(WalletErrors.IsDeleted);
        }

        IsDeleted = true;
        DeletedOnUtc = deletedOnUtc;

        return this;
    }

    private void AddOperation(
        WalletHistoryType type,
        string description,
        Money amount,
        DateOnly date)
    {
        _walletHistory.Add(WalletHistory.Create(
            this,
            type,
            description,
            amount,
            AvailableAmount,
            SavingsAmount,
            date));
    }
}