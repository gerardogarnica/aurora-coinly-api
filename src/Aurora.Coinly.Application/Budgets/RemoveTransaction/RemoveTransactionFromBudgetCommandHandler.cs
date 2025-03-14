using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Budgets.RemoveTransaction;

internal sealed class RemoveTransactionFromBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository) : ICommandHandler<RemoveTransactionFromBudgetCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionFromBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get budget
        var budget = await budgetRepository.GetByIdAsync(request.BudgetId);
        if (budget is null)
        {
            return Result.Fail(BudgetErrors.NotFound);
        }

        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Remove transaction from budget
        var result = budget.RemoveTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}