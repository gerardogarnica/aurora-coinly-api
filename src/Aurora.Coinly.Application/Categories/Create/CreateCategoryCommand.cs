using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Categories.Create;

public sealed record CreateCategoryCommand(
    string Name,
    TransactionType Type,
    int MaxDaysToReverse,
    string Color,
    CategoryGroup Group,
    string? Notes) : ICommand<Guid>;