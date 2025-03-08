namespace Aurora.Coinly.Domain.Methods;

public sealed class PaymentMethod : BaseEntity
{
    public string Name { get; private set; }
    public bool IsDefault { get; private set; }
    public bool AllowRecurring { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }

    private PaymentMethod() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        IsDefault = false;
        AllowRecurring = false;
    }

    public static PaymentMethod Create(
        string name,
        bool isDefault,
        bool allowRecurring,
        string? notes)
    {
        var paymentMethod = new PaymentMethod
        {
            Name = name,
            IsDefault = isDefault,
            AllowRecurring = allowRecurring,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        return paymentMethod;
    }

    public Result<PaymentMethod> Update(
        string name,
        bool allowRecurring,
        string? notes)
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        Name = name;
        AllowRecurring = allowRecurring;
        Notes = notes;
        UpdatedOnUtc = DateTime.UtcNow;

        return this;
    }

    public Result<PaymentMethod> Delete()
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;

        return this;
    }

    public Result<PaymentMethod> SetAsDefault()
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDefault = true;
        UpdatedOnUtc = DateTime.UtcNow;

        return this;
    }

    public Result<PaymentMethod> SetAsNotDefault()
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDefault = false;
        UpdatedOnUtc = DateTime.UtcNow;

        return this;
    }
}