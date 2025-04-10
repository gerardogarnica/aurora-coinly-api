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
    public Guid PaymentMethodId { get; private set; }
    public Guid? WalletId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRecurring => InstallmentNumber > 0;
    public int InstallmentNumber { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? PaidOnUtc { get; private set; }
    public DateTime? RemovedOnUtc { get; private set; }
    public Category Category { get; init; } = null!;
    public PaymentMethod PaymentMethod { get; init; } = null!;

    private Transaction() : base(Guid.NewGuid())
    {
        Description = string.Empty;
        Amount = Money.Zero();
    }

    public static Result<Transaction> Create(
        string description,
        Category category,
        DateOnly transactionDate,
        DateOnly maxPaymentDate,
        Money amount,
        PaymentMethod paymentMethod,
        string? notes,
        int installmentNumber,
        DateTime createdOnUtc)
    {
        if (category.IsDeleted)
        {
            return Result.Fail<Transaction>(CategoryErrors.IsDeleted);
        }

        if (paymentMethod.IsDeleted)
        {
            return Result.Fail<Transaction>(PaymentMethodErrors.IsDeleted);
        }

        var transaction = new Transaction
        {
            Description = description,
            CategoryId = category.Id,
            TransactionDate = transactionDate,
            MaxPaymentDate = maxPaymentDate,
            Amount = amount,
            PaymentMethodId = paymentMethod.Id,
            Notes = notes,
            InstallmentNumber = installmentNumber,
            Status = TransactionStatus.Pending,
            CreatedOnUtc = createdOnUtc
        };

        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction));

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

        AddDomainEvent(new TransactionRemovedEvent(this));

        return this;
    }

    public Result<Transaction> UndoPayment()
    {
        if (Status != TransactionStatus.Paid)
        {
            return Result.Fail<Transaction>(TransactionErrors.NotPaid);
        }

        Status = TransactionStatus.Pending;
        PaymentDate = null;
        WalletId = null;
        PaidOnUtc = null;

        AddDomainEvent(new TransactionPaymentUndoneEvent(this));

        return this;
    }
}