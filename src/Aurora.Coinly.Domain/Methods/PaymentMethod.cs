﻿namespace Aurora.Coinly.Domain.Methods;

public sealed class PaymentMethod : BaseEntity
{
    public string Name { get; private set; }
    public bool IsDefault { get; private set; }
    public bool AllowRecurring { get; private set; }
    public bool AutoMarkAsPaid { get; private set; }
    public Guid RelatedWalletId { get; private set; }
    public int? SuggestedPaymentDay { get; private set; }
    public int? StatementCutoffDay { get; private set; }
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
        AutoMarkAsPaid = false;
    }

    public static PaymentMethod Create(
        string name,
        bool isDefault,
        bool allowRecurring,
        bool autoMarkAsPaid,
        Guid relatedWalletId,
        int? suggestedPaymentDay,
        int? statementCutoffDay,
        string? notes,
        DateTime createdOnUtc)
    {
        var paymentMethod = new PaymentMethod
        {
            Name = name,
            IsDefault = isDefault,
            AllowRecurring = allowRecurring,
            AutoMarkAsPaid = autoMarkAsPaid,
            RelatedWalletId = relatedWalletId,
            SuggestedPaymentDay = suggestedPaymentDay,
            StatementCutoffDay = statementCutoffDay,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = createdOnUtc
        };

        return paymentMethod;
    }

    public Result<PaymentMethod> Update(
        string name,
        bool allowRecurring,
        bool autoMarkAsPaid,
        Guid relatedWalletId,
        int? suggestedPaymentDay,
        int? statementCutoffDay,
        string? notes,
        DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        Name = name;
        AllowRecurring = allowRecurring;
        AutoMarkAsPaid = autoMarkAsPaid;
        RelatedWalletId = relatedWalletId;
        SuggestedPaymentDay = suggestedPaymentDay;
        StatementCutoffDay = statementCutoffDay;
        Notes = notes;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<PaymentMethod> Delete(DateTime deletedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDeleted = true;
        DeletedOnUtc = deletedOnUtc;

        return this;
    }

    public Result<PaymentMethod> SetAsDefault(DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDefault = true;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<PaymentMethod> SetAsNotDefault(DateTime updatedOnUtc)
    {
        if (IsDeleted)
        {
            return Result.Fail<PaymentMethod>(PaymentMethodErrors.IsDeleted);
        }

        IsDefault = false;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }
}