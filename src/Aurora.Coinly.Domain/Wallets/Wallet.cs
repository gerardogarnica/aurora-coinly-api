using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Wallets;

public sealed class Wallet : BaseEntity
{
    private readonly List<WalletHistory> _operations = [];
    private readonly List<PaymentMethod> _methods = [];

    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Money AvailableAmount { get; private set; }
    public Money SavingsAmount { get; private set; }
    public Money TotalAmount => AvailableAmount + SavingsAmount;
    public WalletType Type { get; private set; }
    public bool AllowNegative { get; private set; }
    public Color Color { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateOnly OpenedOn { get; private set; }
    public DateOnly LastOperationOn { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public IReadOnlyCollection<WalletHistory> Operations => _operations.AsReadOnly();
    public IReadOnlyCollection<PaymentMethod> Methods => _methods.AsReadOnly();

    private Wallet() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        AvailableAmount = Money.Zero();
        SavingsAmount = Money.Zero();
        Type = WalletType.Cash;
    }

    public static Wallet Create(
        Guid userId,
        string name,
        Money amount,
        WalletType type,
        bool allowNegative,
        Color color,
        string? notes,
        DateOnly openedOn,
        DateTime createdOn)
    {
        var wallet = new Wallet
        {
            UserId = userId,
            Name = name,
            AvailableAmount = new Money(amount.Amount, amount.Currency),
            SavingsAmount = Money.Zero(amount.Currency),
            Type = type,
            AllowNegative = allowNegative,
            Color = color,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = createdOn
        };

        wallet.AddOperation(
            WalletHistoryType.Created,
            "Wallet created",
            amount,
            openedOn,
            createdOn);

        return wallet;
    }

    public Result<Wallet> Update(
        string name,
        bool allowNegative,
        Color color,
        string? notes,
        DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<Wallet>(WalletErrors.IsDeleted);
        }

        Name = name;
        AllowNegative = allowNegative;
        Color = color;
        Notes = notes;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<Wallet> AssignToSavings(Money amount, DateOnly assignedOn, DateTime updatedOnUtc)
    {
        if (!IsAvailableForWithdrawal(amount))
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
            assignedOn,
            updatedOnUtc);

        AddDomainEvent(new WalletSavingsUpdatedEvent(Id, amount, assignedOn, true));

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
            assignedOn,
            updatedOnUtc);

        AddDomainEvent(new WalletSavingsUpdatedEvent(Id, amount, assignedOn, false));

        return this;
    }

    public Result<Wallet> Deposit(Transaction transaction, DateTime updatedOnUtc)
    {
        if (!transaction.IsPaid)
        {
            return Result.Fail<Wallet>(TransactionErrors.NotPaid);
        }

        return Deposit(
            transaction.Amount,
            transaction.Description,
            transaction.PaymentDate!.Value,
            transaction.Id,
            updatedOnUtc);
    }

    public Result<Wallet> Deposit(
        Money amount,
        string description,
        DateOnly processedOn,
        DateTime updatedOnUtc)
    {
        return Deposit(
            amount,
            description,
            processedOn,
            null,
            updatedOnUtc);
    }

    private Result<Wallet> Deposit(
        Money amount,
        string description,
        DateOnly processedOn,
        Guid? transactionId,
        DateTime updatedOnUtc)
    {
        AvailableAmount += amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.Deposit,
            description,
            amount,
            processedOn,
            updatedOnUtc,
            transactionId);

        AddDomainEvent(new WalletBalanceUpdatedEvent(Id));

        return this;
    }

    public Result<Wallet> Withdraw(Transaction transaction, DateTime updatedOnUtc)
    {
        if (!transaction.IsPaid)
        {
            return Result.Fail<Wallet>(TransactionErrors.NotPaid);
        }

        return Withdraw(
            transaction.Amount,
            transaction.Description,
            transaction.PaymentDate!.Value,
            transaction.Id,
            updatedOnUtc);
    }

    public Result<Wallet> Withdraw(
        Money amount,
        string description,
        DateOnly processedOn,
        DateTime updatedOnUtc)
    {
        return Withdraw(
            amount,
            description,
            processedOn,
            null,
            updatedOnUtc);
    }

    private Result<Wallet> Withdraw(
        Money amount,
        string description,
        DateOnly processedOn,
        Guid? transactionId,
        DateTime updatedOnUtc)
    {
        AvailableAmount -= amount;
        UpdatedOnUtc = updatedOnUtc;

        AddOperation(
            WalletHistoryType.Withdrawal,
            description,
            amount,
            processedOn,
            updatedOnUtc,
            transactionId);

        AddDomainEvent(new WalletBalanceUpdatedEvent(Id));

        return this;
    }

    public Result<Wallet> Delete(DateTime deletedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<Wallet>(WalletErrors.IsDeleted);
        }

        if (_methods.Any(x => !x.IsDeleted))
        {
            return Result.Fail<Wallet>(WalletErrors.HasActivePaymentMethods);
        }

        IsDeleted = true;
        DeletedOnUtc = deletedOnUtc;

        return this;
    }

    public Result<Wallet> RemoveTransaction(Transaction transaction)
    {
        if (IsDeleted)
        {
            return Result.Fail<Wallet>(WalletErrors.IsDeleted);
        }

        if (!_operations.Any(t => t.TransactionId == transaction.Id))
        {
            return Result.Fail<Wallet>(WalletErrors.TransactionNotBelongs);
        }

        if (transaction.IsPaid)
        {
            return Result.Fail<Wallet>(TransactionErrors.AlreadyPaid);
        }

        // Get the operation associated with the transaction
        var operation = _operations.First(t => t.TransactionId == transaction.Id);

        AvailableAmount = operation.IsIncrement
            ? AvailableAmount - operation.Amount
            : AvailableAmount + operation.Amount;

        _operations.Remove(operation);

        AddDomainEvent(new WalletBalanceUpdatedEvent(Id));

        return this;
    }

    public void SetOpenAndLastOperationDates(DateOnly openedOn, DateOnly lastOperationOn)
    {
        OpenedOn = openedOn;
        LastOperationOn = lastOperationOn;
    }

    public void SetOperations(IList<WalletHistory> operations)
    {
        _operations.Clear();

        _operations.AddRange(operations);
    }

    private void AddOperation(
        WalletHistoryType type,
        string description,
        Money amount,
        DateOnly date,
        DateTime createdOn,
        Guid? transactionId = null)
    {
        var operation = WalletHistory.Create(
            Id,
            transactionId,
            type,
            description,
            date,
            new Money(amount.Amount, amount.Currency),
            new Money(AvailableAmount.Amount, AvailableAmount.Currency),
            new Money(SavingsAmount.Amount, SavingsAmount.Currency),
            createdOn);

        _operations.Add(operation);
    }

    internal bool IsAvailableForWithdrawal(Money amount)
    {
        if (AvailableAmount >= amount)
        {
            return true;
        }

        return AllowNegative;
    }
}