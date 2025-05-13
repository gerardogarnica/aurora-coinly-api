using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets;

public sealed record BudgetTransactionModel(
    Guid TransactionId,
    string Description,
    DateOnly Date,
    decimal Amount);

internal static class BudgetTransactionModelExtensions
{
    internal static BudgetTransactionModel ToModel(this BudgetTransaction budgetTransaction) => new(
        budgetTransaction.Id,
        budgetTransaction.Description,
        budgetTransaction.TransactionDate,
        budgetTransaction.Amount.Amount);
}