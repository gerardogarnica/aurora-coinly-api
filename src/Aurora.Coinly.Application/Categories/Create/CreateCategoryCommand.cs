using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Categories.Create;

public sealed record CreateCategoryCommand(
    string Name,
    CategoryGroup Group,
    TransactionType Type,
    int MaxDaysToReverse,
    string Color,
    string? Notes) : ICommand<Guid>;