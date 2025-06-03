using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Categories;

public sealed record CategoryModel(
    Guid CategoryId,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    int MaxDaysToReverse,
    string Color,
    bool IsDeleted,
    string? Notes);

internal static class CategoryModelExtensions
{
    internal static CategoryModel ToModel(this Category category) => new(
        category.Id,
        category.Name,
        category.Type,
        category.MaxDaysToReverse,
        category.Color.Value,
        category.IsDeleted,
        category.Notes);
}