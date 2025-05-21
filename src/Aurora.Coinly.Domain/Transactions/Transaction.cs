using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Domain.Transactions;

public sealed class Transaction : BaseEntity
{
    public string Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public DateOnly MaxPaymentDate { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public DateOnly QueryDate { get; private set; }
    public TransactionType Type => Category.Type;
    public bool IsPaid => Status == TransactionStatus.Paid;
    public Money Amount { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public Guid? WalletId { get; private set; }
    public TransactionStatus Status { get; private set; }
    public bool IsRecurring => InstallmentNumber > 0;
    public int InstallmentNumber { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? PaidOnUtc { get; private set; }
    public DateTime? RemovedOnUtc { get; private set; }
    public Category Category { get; init; } = null!;
    public PaymentMethod? PaymentMethod { get; init; } = null!;
    public Wallet? Wallet { get; init; } = null!;

    private Transaction() : base(Guid.NewGuid()) { }

    public static Result<Transaction> Create(
        string description,
        Category category,
        DateOnly transactionDate,
        DateOnly maxPaymentDate,
        Money amount,
        PaymentMethod? paymentMethod,
        string? notes,
        DateTime createdOnUtc)
    {
        if (category.IsDeleted)
        {
            return Result.Fail<Transaction>(CategoryErrors.IsDeleted);
        }

        var transaction = new Transaction
        {
            Description = description,
            CategoryId = category.Id,
            TransactionDate = transactionDate,
            MaxPaymentDate = maxPaymentDate,
            QueryDate = maxPaymentDate,
            Amount = amount,
            PaymentMethodId = paymentMethod?.Id,
            WalletId = paymentMethod?.WalletId,
            Status = TransactionStatus.Pending,
            InstallmentNumber = 0,
            Notes = notes,
            CreatedOnUtc = createdOnUtc
        };

        return transaction;
    }

    public Result<Transaction> Pay(Wallet wallet, DateOnly paymentDate, DateTime paidOnUtc)
    {
        if (Status != TransactionStatus.Pending)
        {
            return Result.Fail<Transaction>(TransactionErrors.NotPending);
        }

        if (wallet.IsDeleted)
        {
            return Result.Fail<Transaction>(WalletErrors.IsDeleted);
        }

        if (Type is TransactionType.Expense && wallet.IsAvailableForWithdrawal(Amount))
        {
            return Result.Fail<Transaction>(WalletErrors.InsufficientFunds);
        }

        if (paymentDate < TransactionDate)
        {
            return Result.Fail<Transaction>(TransactionErrors.InvalidPaymentDate(TransactionDate));
        }

        PaymentDate = paymentDate;
        QueryDate = paymentDate;
        Status = TransactionStatus.Paid;
        WalletId = wallet.Id;
        PaidOnUtc = paidOnUtc;

        AddDomainEvent(new TransactionPaidEvent(this));

        return this;
    }

    public Result<Transaction> Remove(DateTime removedOnUtc)
    {
        if (Status == TransactionStatus.Paid)
        {
            return Result.Fail<Transaction>(TransactionErrors.AlreadyPaid);
        }

        if (Status == TransactionStatus.Removed)
        {
            return Result.Fail<Transaction>(TransactionErrors.AlreadyRemoved);
        }

        Status = TransactionStatus.Removed;
        RemovedOnUtc = removedOnUtc;

        return this;
    }

    public Result<Transaction> UndoPayment(DateTime unpaidOnUtc)
    {
        if (Status != TransactionStatus.Paid)
        {
            return Result.Fail<Transaction>(TransactionErrors.NotPaid);
        }

        if (Type is TransactionType.Income && Wallet!.IsAvailableForWithdrawal(Amount))
        {
            return Result.Fail<Transaction>(WalletErrors.InsufficientFunds);
        }

        if (Type is TransactionType.Expense && !PaymentMethod!.IsAvailableToReverse(PaymentDate!.Value, unpaidOnUtc))
        {
            return Result.Fail<Transaction>(PaymentMethodErrors.IsUnavailableToReverse);
        }

        PaymentDate = null;
        QueryDate = MaxPaymentDate;
        Status = TransactionStatus.Pending;
        PaidOnUtc = null;

        AddDomainEvent(new TransactionPaymentUndoneEvent(this));

        return this;
    }
}