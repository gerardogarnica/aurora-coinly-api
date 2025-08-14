using Aurora.Coinly.Domain.UnitTests.Users;

namespace Aurora.Coinly.Domain.UnitTests.Categories;

internal static class CategoryData
{
    public const string Name = "Test Category";
    public const TransactionType Type = TransactionType.Expense;
    public const int MaxDaysToReverse = 10;
    public static readonly Color Color = Color.FromHex("#000000");
    public const CategoryGroup Group = CategoryGroup.Other;
    public const string? Notes = "Notes of the category";

    public static Category GetCategory() => Category.Create(
        UserData.GetUser().Id,
        Name,
        Group,
        Type,
        MaxDaysToReverse,
        Color,
        Notes,
        DateTime.UtcNow);
}