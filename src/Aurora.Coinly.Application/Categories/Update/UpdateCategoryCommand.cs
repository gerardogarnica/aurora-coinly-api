using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.Update;

public sealed record UpdateCategoryCommand(
    Guid Id,
    Guid UserId,
    string Name,
    int MaxDaysToReverse,
    string Color,
    CategoryGroup Group,
    string? Notes) : ICommand;