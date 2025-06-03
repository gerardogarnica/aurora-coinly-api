using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Categories.Create;

public sealed record CreateCategoryCommand(
    string Name,
    TransactionType Type,
    int MaxDaysToReverse,
    string Color,
    string? Notes) : ICommand<Guid>;