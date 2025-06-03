using Aurora.Coinly.Application.Categories;
using Aurora.Coinly.Application.Methods;
using Aurora.Coinly.Application.Wallets;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions;

public sealed record TransactionModel(
    Guid TransactionId,
    string Description,
    CategoryModel? Category,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    DateOnly? PaymentDate,
    DateOnly DisplayDate,
    DisplayDateType DisplayDateType,
    string Currency,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionStatus Status,
    bool IsPaid,
    PaymentMethodModel? PaymentMethod,
    WalletModel? Wallet,
    string? Notes,
    bool IsRecurring,
    int InstallmentNumber);

internal static class TransactionModelExtensions
{
    internal static TransactionModel ToModel(this Transaction transaction, DisplayDateType displayDateType) => new(
        transaction.Id,
        transaction.Description,
        transaction.Category?.ToModel(),
        transaction.TransactionDate,
        transaction.MaxPaymentDate,
        transaction.PaymentDate,
        GetDisplayDate(transaction, displayDateType),
        displayDateType,
        transaction.Amount.Currency.Code,
        transaction.Amount.Amount,
        transaction.Type,
        transaction.Status,
        transaction.IsPaid,
        transaction.PaymentMethod?.ToModel(),
        transaction.Wallet?.ToModel(),
        transaction.Notes,
        transaction.IsRecurring,
        transaction.InstallmentNumber);

    private static DateOnly GetDisplayDate(Transaction transaction, DisplayDateType displayDateType)
    {
        return displayDateType == DisplayDateType.TransactionDate
            ? transaction.TransactionDate
            : transaction.PaymentDate ?? transaction.TransactionDate;
    }
}