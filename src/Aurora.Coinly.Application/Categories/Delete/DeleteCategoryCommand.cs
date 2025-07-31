namespace Aurora.Coinly.Application.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id, Guid UserId) : ICommand;