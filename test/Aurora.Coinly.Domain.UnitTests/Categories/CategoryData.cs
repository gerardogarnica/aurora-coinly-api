using Aurora.Coinly.Domain.UnitTests.Users;

namespace Aurora.Coinly.Domain.UnitTests.Categories;

internal static class CategoryData
{
    public const string Name = "Test Category";
    public const int MaxDaysToReverse = 10;
    public static readonly Color Color = Color.FromHex("#000000");
    public const CategoryGroup Group = CategoryGroup.Other;
    public const string? Notes = "Notes of the category";

    public static Category GetCategory(TransactionType type = TransactionType.Expense) => Category.Create(
        UserData.GetUser().Id,
        Name,
        Group,
        type,
        MaxDaysToReverse,
        Color,
        Notes,
        DateTime.UtcNow);
}