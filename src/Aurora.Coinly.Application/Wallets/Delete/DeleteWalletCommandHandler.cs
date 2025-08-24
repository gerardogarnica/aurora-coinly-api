namespace Aurora.Coinly.Application.Wallets.Delete;

internal sealed class DeleteWalletCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<DeleteWalletCommand>
{
    public async Task<Result> Handle(
        DeleteWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Update wallet
        var result = wallet.Delete(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Wallets.Update(wallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}