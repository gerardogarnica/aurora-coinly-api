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
            Amount = amount,
            PaymentMethodId = paymentMethod?.Id,
            WalletId = paymentMethod?.WalletId,
            Status = TransactionStatus.Pending,
            InstallmentNumber = 0,
            Notes = notes,
            CreatedOnUtc = createdOnUtc,
            Category = category,
            PaymentMethod = paymentMethod
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

        if (!WalletIsAvailableForPayment(wallet))
        {
            return Result.Fail<Transaction>(WalletErrors.InsufficientFunds);
        }

        if (paymentDate < TransactionDate)
        {
            return Result.Fail<Transaction>(TransactionErrors.InvalidPaymentDate(TransactionDate));
        }

        PaymentDate = paymentDate;
        Status = TransactionStatus.Paid;
        WalletId = wallet.Id;
        PaidOnUtc = paidOnUtc;

        AddDomainEvent(new TransactionPaidEvent(this));

        return this;
    }

    public Result<Transaction> Remove(DateTime removedOnUtc, DateOnly today)
    {
        if (Status == TransactionStatus.Paid)
        {
            return Result.Fail<Transaction>(TransactionErrors.AlreadyPaid);
        }

        if (Status == TransactionStatus.Removed)
        {
            return Result.Fail<Transaction>(TransactionErrors.AlreadyRemoved);
        }

        if (!CategoryIsAvailableToReverse(TransactionDate, today))
        {
            return Result.Fail<Transaction>(CategoryErrors.IsUnavailableToReverse);
        }

        Status = TransactionStatus.Removed;
        RemovedOnUtc = removedOnUtc;

        return this;
    }

    public Result<Transaction> UndoPayment(DateTime unpaidOnUtc, DateOnly today)
    {
        if (Status != TransactionStatus.Paid)
        {
            return Result.Fail<Transaction>(TransactionErrors.NotPaid);
        }

        if (!WalletIsAvailableForUndoPayment())
        {
            return Result.Fail<Transaction>(WalletErrors.InsufficientFunds);
        }

        if (!CategoryIsAvailableToReverse(PaymentDate!.Value, today))
        {
            return Result.Fail<Transaction>(CategoryErrors.IsUnavailableToReverse);
        }

        if (!PaymentMethodIsAvailableToReverse(today))
        {
            return Result.Fail<Transaction>(PaymentMethodErrors.IsUnavailableToReverse);
        }

        var paymentDate = PaymentDate.Value;

        PaymentDate = null;
        Status = TransactionStatus.Pending;
        PaidOnUtc = null;
        WalletId = null;
        RemovedOnUtc = unpaidOnUtc;

        AddDomainEvent(new TransactionUnpaidEvent(this, paymentDate));

        return this;
    }

    private bool WalletIsAvailableForPayment(Wallet wallet)
    {
        return Type switch
        {
            TransactionType.Expense => wallet.IsAvailableForWithdrawal(Amount),
            TransactionType.Income => true,
            _ => false
        };
    }

    private bool WalletIsAvailableForUndoPayment()
    {
        return Type switch
        {
            TransactionType.Income => Wallet?.IsAvailableForWithdrawal(Amount) ?? false,
            TransactionType.Expense => true,
            _ => false
        };
    }

    private bool CategoryIsAvailableToReverse(DateOnly dateToCompare, DateOnly today)
    {
        return Category.IsAvailableToReverse(dateToCompare, today);
    }

    private bool PaymentMethodIsAvailableToReverse(DateOnly today)
    {
        // Income transactions do not have a payment method to reverse
        if (Type is TransactionType.Income)
        {
            return true;
        }

        return PaymentMethod?.IsAvailableToReverse(PaymentDate!.Value, today) ?? false;
    }
}