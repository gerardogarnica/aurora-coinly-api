namespace Aurora.Coinly.Application.Summary.UpdateSavings;

internal sealed class UpdateSummarySavingsCommandHandler(
    ICoinlyDbContext dbContext) : ICommandHandler<UpdateSummarySavingsCommand>
{
    public async Task<Result> Handle(
        UpdateSummarySavingsCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == request.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Check if summary exists for the requested year
        var existsSummary = await dbContext
            .MonthlySummaries
            .AnyAsync(x => x.UserId == wallet.UserId && x.Year == request.AssignedOn.Year, cancellationToken);

        List<MonthlySummary> summaries = [];
        if (!existsSummary)
        {
            summaries = [.. MonthlySummary.Create(
                wallet.UserId,
                request.AssignedOn.Year,
                wallet.TotalAmount.Currency.Code)];
        }

        // Get monthly summary
        MonthlySummary? monthlySummary = existsSummary
            ? await dbContext
                .MonthlySummaries
                .SingleOrDefaultAsync(x =>
                    x.UserId == wallet.UserId &&
                    x.Year == request.AssignedOn.Year &&
                    x.Month == request.AssignedOn.Month &&
                    x.CurrencyCode == request.Amount.Currency.Code,
                    cancellationToken)
            : summaries.Find(x => x.Month == request.AssignedOn.Month);

        if (monthlySummary is null)
        {
            return Result.Fail(SummaryErrors.NotFound);
        }

        // Update savings
        var result = monthlySummary.UpdateSavings(request.Amount, request.AssignedOn, request.IsIncrement);

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