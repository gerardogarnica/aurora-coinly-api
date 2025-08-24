namespace Aurora.Coinly.Application.Summary.AddTransaction;

internal sealed class AddSummaryTransactionCommandHandler(
    ICoinlyDbContext dbContext) : ICommandHandler<AddSummaryTransactionCommand>
{
    public async Task<Result> Handle(
        AddSummaryTransactionCommand request,
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

        // Get monthly summary
        MonthlySummary? monthlySummary = await dbContext
            .MonthlySummaries
            .SingleOrDefaultAsync(x =>
                x.UserId == transaction.UserId &&
                x.Year == transaction.PaymentDate!.Value.Year &&
                x.Month == transaction.PaymentDate!.Value.Month &&
                x.Currency.Code == transaction.Amount.Currency.Code,
                cancellationToken);

        var isNewSummary = monthlySummary is null;

        monthlySummary ??= MonthlySummary.Create(
            transaction.UserId,
            transaction.PaymentDate!.Value.Year,
            transaction.PaymentDate!.Value.Month,
            transaction.Amount.Currency);

        // Apply transaction to summary
        var result = monthlySummary.ApplyTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        if (isNewSummary)
        {
            dbContext.MonthlySummaries.Add(result.Value);
        }
        else
        {
            dbContext.MonthlySummaries.Update(result.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}