using Aurora.Coinly.Domain.UnitTests.Users;
using Aurora.Coinly.Domain.UnitTests.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Methods;

internal static class PaymentMethodData
{
    public const string Name = "Test payment method";
    public const bool IsDefault = false;
    public const bool AllowRecurring = false;
    public const bool AutoMarkAsPaid = false;
    public const int MaxDaysToReverse = 10;
    public const int SuggestedPaymentDay = 15;
    public const int StatementCutoffDay = 31;
    public const string? Notes = "Notes of the payment method";

    public static PaymentMethod GetPaymentMethod() => PaymentMethod.Create(
        UserData.GetUser().Id,
        Name,
        IsDefault,
        AllowRecurring,
        AutoMarkAsPaid,
        WalletData.GetWallet(),
        MaxDaysToReverse,
        SuggestedPaymentDay,
        StatementCutoffDay,
        Notes,
        DateTime.UtcNow);
}