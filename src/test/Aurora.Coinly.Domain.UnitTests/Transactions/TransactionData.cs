namespace Aurora.Coinly.Domain.UnitTests.Transactions;

internal static class TransactionData
{
    public const string Description = "Description of the transaction";
    public static readonly Money Amount = new(10.0m, Currency.Usd);
    public const string? Notes = "Notes of the category";
    public const int InstallmentNumber = 0;

    public static Transaction GetTransaction(Category category, PaymentMethod paymentMethod) => Transaction.Create(
        Description,
        category,
        DateOnly.FromDateTime(DateTime.UtcNow),
        DateOnly.FromDateTime(DateTime.UtcNow),
        Amount,
        paymentMethod,
        Notes,
        InstallmentNumber,
        DateTime.UtcNow).Value;
}