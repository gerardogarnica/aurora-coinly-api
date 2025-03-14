using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Categories.Create;

public sealed record CreateCategoryCommand(
    string Name,
    TransactionType Type,
    string? Notes) : ICommand<Guid>;