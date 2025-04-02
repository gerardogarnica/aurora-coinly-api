using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions;

public sealed record TransactionModel(
    Guid TransactionId,
    string Description,
    Guid CategoryId,
    string CategoryName,
    Guid PaymentMethodId,
    string PaymentMethodName,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    DateOnly? PaymentDate,
    string Currency,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionStatus Status,
    bool IsPaid,
    Guid? WalletId,
    string? Notes,
    bool IsRecurring,
    int InstallmentNumber);

internal static class TransactionModelExtensions
{
    internal static TransactionModel ToModel(this Transaction transaction) => new(
        transaction.Id,
        transaction.Description,
        transaction.Category.Id,
        transaction.Category.Name,
        transaction.PaymentMethod.Id,
        transaction.PaymentMethod.Name,
        transaction.TransactionDate,
        transaction.MaxPaymentDate,
        transaction.PaymentDate,
        transaction.Amount.Currency.Code,
        transaction.Amount.Amount,
        transaction.Type,
        transaction.Status,
        transaction.IsPaid,
        transaction.WalletId,
        transaction.Notes,
        transaction.IsRecurring,
        transaction.InstallmentNumber);
}