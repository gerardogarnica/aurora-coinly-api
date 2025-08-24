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

        // Get monthly summary
        MonthlySummary? monthlySummary = await dbContext
            .MonthlySummaries
            .SingleOrDefaultAsync(x =>
                x.UserId == wallet.UserId &&
                x.Year == request.AssignedOn.Year &&
                x.Month == request.AssignedOn.Month &&
                x.Currency.Code == request.Amount.Currency.Code,
                cancellationToken);

        var isNewSummary = monthlySummary is null;

        monthlySummary ??= MonthlySummary.Create(
            wallet.UserId,
            request.AssignedOn.Year,
            request.AssignedOn.Month,
            request.Amount.Currency);

        // Update savings
        var result = monthlySummary.UpdateSavings(request.Amount, request.AssignedOn, request.IsIncrement);

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