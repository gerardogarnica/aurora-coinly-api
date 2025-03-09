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
    public TransactionType Type => Category.Type;
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
        CategoryId = Guid.Empty;
        TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        MaxPaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        Amount = Money.Zero();
        PaymentMethodId = Guid.Empty;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public static Transaction Create(
        string description,
        Category category,
        DateOnly transactionDate,
        DateOnly maxPaymentDate,
        Money amount,
        PaymentMethod paymentMethod,
        string? notes,
        int installmentNumber,
        Wallet? wallet)
    {
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
            Status = paymentMethod.AutoMarkAsPaid ? TransactionStatus.Paid : TransactionStatus.Pending,
            WalletId = wallet?.Id
        };

        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction));

        return transaction;
    }

    public Result<Transaction> Pay(Wallet wallet)
    {
        if (Status != TransactionStatus.Pending)
        {
            return Result.Fail<Transaction>(TransactionErrors.NotPending);
        }

        Status = TransactionStatus.Paid;
        WalletId = wallet.Id;
        PaidOnUtc = DateTime.UtcNow;

        AddDomainEvent(new TransactionPaidEvent(this));

        return this;
    }

    public Result<Transaction> Remove()
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
        RemovedOnUtc = DateTime.UtcNow;

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
        WalletId = null;
        PaidOnUtc = null;

        AddDomainEvent(new TransactionPaymentUndoneEvent(this));

        return this;
    }
}