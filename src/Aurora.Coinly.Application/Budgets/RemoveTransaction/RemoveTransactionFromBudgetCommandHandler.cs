namespace Aurora.Coinly.Application.Budgets.RemoveTransaction;

internal sealed class RemoveTransactionFromBudgetCommandHandler(
    ICoinlyDbContext dbContext) : ICommandHandler<RemoveTransactionFromBudgetCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionFromBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        Transaction? transaction = await dbContext
            .Transactions
            .SingleOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Get budget
        Budget? budget = await dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .ThenInclude(x => x.Transactions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.UserId == transaction.UserId && x.CategoryId == transaction.CategoryId && x.Year == request.OriginalPaymentDate.Year, cancellationToken);

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

        dbContext.Budgets.Update(budget);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}