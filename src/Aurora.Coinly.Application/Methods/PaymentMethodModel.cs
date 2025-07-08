using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods;

public sealed record PaymentMethodModel(
    Guid PaymentMethodId,
    WalletModel? Wallet,
    string Name,
    bool IsDefault,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    int MaxDaysToReverse,
    int? SuggestedPaymentDay,
    int? StatementCutoffDay,
    bool IsDeleted,
    string? Notes);

internal static class PaymentMethodModelExtensions
{
    internal static PaymentMethodModel ToModel(this PaymentMethod paymentMethod) => new(
        paymentMethod.Id,
        paymentMethod.Wallet?.ToModel(),
        paymentMethod.Name,
        paymentMethod.IsDefault,
        paymentMethod.AllowRecurring,
        paymentMethod.AutoMarkAsPaid,
        paymentMethod.MaxDaysToReverse,
        paymentMethod.SuggestedPaymentDay,
        paymentMethod.StatementCutoffDay,
        paymentMethod.IsDeleted,
        paymentMethod.Notes);
}