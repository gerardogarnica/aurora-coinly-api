namespace Aurora.Coinly.Application.Categories.Update;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Color,
    string? Notes) : ICommand;