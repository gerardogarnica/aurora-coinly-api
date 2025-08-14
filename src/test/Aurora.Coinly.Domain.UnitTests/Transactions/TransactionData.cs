using Aurora.Coinly.Domain.UnitTests.Users;

namespace Aurora.Coinly.Domain.UnitTests.Transactions;

internal static class TransactionData
{
    public const string Description = "Description of the transaction";
    public static readonly Money Amount = new(10.0m, Currency.Usd);
    public const string? Notes = "Notes of the category";

    public static Transaction GetTransaction(Category category, PaymentMethod paymentMethod) => Transaction.Create(
        UserData.GetUser().Id,
        Description,
        category,
        DateOnly.FromDateTime(DateTime.UtcNow),
        DateOnly.FromDateTime(DateTime.UtcNow),
        Amount,
        paymentMethod,
        Notes,
        DateTime.UtcNow).Value;
}