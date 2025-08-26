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
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail(TransactionErrors.NotPaid);
        }

        // Check if summary exists for the requested year
        var existsSummary = await dbContext
            .MonthlySummaries
            .AnyAsync(x => x.UserId == transaction.UserId && x.Year == transaction.PaymentDate!.Value.Year, cancellationToken);

        List<MonthlySummary> summaries = [];
        if (!existsSummary)
        {
            summaries = [.. MonthlySummary.Create(
                transaction.UserId,
                transaction.PaymentDate!.Value.Year,
                transaction.Amount.Currency.Code)];
        }

        // Get monthly summary
        MonthlySummary? monthlySummary = existsSummary
            ? await dbContext
                .MonthlySummaries
                .SingleOrDefaultAsync(x =>
                    x.UserId == transaction.UserId &&
                    x.Year == transaction.PaymentDate!.Value.Year &&
                    x.Month == transaction.PaymentDate!.Value.Month &&
                    x.CurrencyCode == transaction.Amount.Currency.Code,
                    cancellationToken)
            : summaries.Find(x => x.Month == transaction.PaymentDate!.Value.Month);

        if (monthlySummary is null)
        {
            return Result.Fail(SummaryErrors.NotFound);
        }

        // Apply transaction to summary
        var result = monthlySummary.ApplyTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        if (!existsSummary)
        {
            dbContext.MonthlySummaries.AddRange(summaries);
        }
        else
        {
            dbContext.MonthlySummaries.Update(result.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}