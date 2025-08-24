namespace Aurora.Coinly.Application.Categories.Update;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    int MaxDaysToReverse,
    string Color,
    CategoryGroup Group,
    string? Notes) : ICommand;