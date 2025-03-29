namespace Aurora.Coinly.Domain.UnitTests.Categories;

internal static class CategoryData
{
    public const string Name = "Test Category";
    public const TransactionType Type = TransactionType.Expense;
    public const string? Notes = "Notes of the category";

    public static Category GetCategory() => Category.Create(
        Name,
        Type,
        Notes,
        DateTime.UtcNow);
}