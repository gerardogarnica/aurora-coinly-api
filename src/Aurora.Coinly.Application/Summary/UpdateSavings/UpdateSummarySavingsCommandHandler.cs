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

        if (!existsSummary)
        {
            // Get savings from the wallets
            List<Wallet> wallets = await dbContext
                .Wallets
                .Where(x => x.UserId == wallet.UserId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            decimal savingsAmount = wallets.Sum(x => x.SavingsAmount.Amount);

            // Create summaries for all months in the year
            List<MonthlySummary> summaries = [.. MonthlySummary.Create(
                wallet.UserId,
                request.AssignedOn.Year,
                wallet.TotalAmount.Currency.Code,
                savingsAmount)];

            dbContext.MonthlySummaries.AddRange(summaries);
        }
        else
        {
            List<MonthlySummary> summaries = await dbContext
                .MonthlySummaries
                .Where(x =>
                    x.UserId == wallet.UserId &&
                    x.Year == request.AssignedOn.Year &&
                    x.Month >= request.AssignedOn.Month &&
                    x.CurrencyCode == request.Amount.Currency.Code)
                .ToListAsync(cancellationToken);

            summaries.ForEach(x => x.UpdateSavings(request.Amount, request.IsIncrement));

            dbContext.MonthlySummaries.UpdateRange(summaries);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}