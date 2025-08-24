namespace Aurora.Coinly.Application.Wallets.AssignToAvailable;

internal sealed class AssignToAvailableCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<AssignToAvailableCommand>
{
    public async Task<Result> Handle(
        AssignToAvailableCommand request,
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

        // Assign to available
        var result = wallet.AssignToAvailable(
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