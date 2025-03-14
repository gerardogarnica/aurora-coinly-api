namespace Aurora.Coinly.Application.Categories.Update;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Notes) : ICommand;