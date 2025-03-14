using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets;

public sealed record BudgetTransactionModel(
    Guid TransactionId,
    string Description,
    decimal Amount,
    DateOnly Date);

internal static class BudgetTransactionModelExtensions
{
    internal static BudgetTransactionModel ToModel(this BudgetTransaction budgetTransaction) => new(
        budgetTransaction.Id,
        budgetTransaction.Description,
        budgetTransaction.Amount.Amount,
        budgetTransaction.TransactionDate);
}