namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

internal sealed class AssignToSavingsCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<AssignToSavingsCommand>
{
    public async Task<Result> Handle(
        AssignToSavingsCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Assign to savings
        var result = wallet.AssignToSavings(
            new Money(request.Amount, wallet.TotalAmount.Currency),
            request.AssignedOn,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Wallets.Update(wallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}