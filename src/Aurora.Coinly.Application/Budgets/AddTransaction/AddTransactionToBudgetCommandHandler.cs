namespace Aurora.Coinly.Application.Budgets.AddTransaction;

internal sealed class AddTransactionToBudgetCommandHandler(
    ICoinlyDbContext dbContext) : ICommandHandler<AddTransactionToBudgetCommand>
{
    public async Task<Result> Handle(
        AddTransactionToBudgetCommand request,
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

        if (!transaction.IsPaid)
        {
            return Result.Fail(TransactionErrors.NotPaid);
        }

        // Get budget
        Budget? budget = await dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .ThenInclude(x => x.Transactions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.UserId == transaction.UserId && x.CategoryId == transaction.CategoryId && x.Year == transaction.PaymentDate!.Value.Year, cancellationToken);

        if (budget is null)
        {
            // Because budget is not necessary for transaction
            return Result.Ok();
        }

        // Add transaction to budget
        var result = budget.AssignTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.BudgetTransactions.Add(result.Value);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}