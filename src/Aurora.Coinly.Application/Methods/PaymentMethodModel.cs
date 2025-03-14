using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods;

public sealed record PaymentMethodModel(
    Guid PaymentMethodId,
    string Name,
    bool IsDefault,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    bool IsDeleted,
    string? Notes);

internal static class PaymentMethodModelExtensions
{
    internal static PaymentMethodModel ToModel(this PaymentMethod paymentMethod) => new(
        paymentMethod.Id,
        paymentMethod.Name,
        paymentMethod.IsDefault,
        paymentMethod.AllowRecurring,
        paymentMethod.AutoMarkAsPaid,
        paymentMethod.IsDeleted,
        paymentMethod.Notes);
}