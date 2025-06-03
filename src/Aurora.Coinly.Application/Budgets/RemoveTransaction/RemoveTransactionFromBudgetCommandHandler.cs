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
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Get budget
        var budget = await budgetRepository.GetByCategoryIdAsync(
            transaction.CategoryId,
            request.OriginalPaymentDate.Year);
        if (budget is null)
        {
            // Because budget is not necessary for transaction
            return Result.Ok();
        }

        // Remove transaction from budget
        var result = budget.RemoveTransaction(transaction, request.OriginalPaymentDate);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}