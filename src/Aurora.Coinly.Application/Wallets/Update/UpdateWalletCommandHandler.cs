namespace Aurora.Coinly.Application.Wallets.Update;

internal sealed class UpdateWalletCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateWalletCommand>
{
    public async Task<Result> Handle(
        UpdateWalletCommand request,
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

        Result<Color> color = Color.Create(request.Color);
        if (!color.IsSuccessful)
        {
            return Result.Fail(color.Error);
        }

        // Update wallet
        var result = wallet.Update(
            request.Name,
            request.AllowNegative,
            color.Value,
            request.Notes,
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