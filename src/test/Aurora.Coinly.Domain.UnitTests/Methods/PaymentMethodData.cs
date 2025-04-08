using Aurora.Coinly.Domain.UnitTests.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Methods;

internal static class PaymentMethodData
{
    public const string Name = "Test payment method";
    public const bool IsDefault = false;
    public const bool AllowRecurring = false;
    public const bool AutoMarkAsPaid = false;
    public const int SuggestedPaymentDay = 15;
    public const int StatementCutoffDay = 31;
    public const string? Notes = "Notes of the payment method";

    public static PaymentMethod GetPaymentMethod() => PaymentMethod.Create(
        Name,
        IsDefault,
        AllowRecurring,
        AutoMarkAsPaid,
        WalletData.GetWallet().Id,
        SuggestedPaymentDay,
        StatementCutoffDay,
        Notes,
        DateTime.UtcNow);
}