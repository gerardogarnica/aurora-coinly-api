namespace Aurora.Coinly.Application.Wallets.Transfer;

internal sealed class TransferBetweenWalletsCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<TransferBetweenWalletsCommand>
{
    public async Task<Result> Handle(
        TransferBetweenWalletsCommand request,
        CancellationToken cancellationToken)
    {
        // Get source wallet
        Wallet? sourceWallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == request.SourceWalletId && x.UserId == userContext.UserId, cancellationToken);

        if (sourceWallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Get destination wallet
        Wallet? destinationWallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == request.DestinationWalletId && x.UserId == userContext.UserId, cancellationToken);

        if (destinationWallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Check if the currency wallets match
        if (sourceWallet.TotalAmount.Currency != destinationWallet.TotalAmount.Currency)
        {
            return Result.Fail(WalletErrors.CurrenciesNotMatch);
        }

        // Withdraw from source wallet
        var withdrawResult = sourceWallet.Withdraw(
            new Money(request.Amount, sourceWallet.TotalAmount.Currency),
            $"Transfer to {destinationWallet.Name}",
            request.TransferedOn,
            dateTimeService.UtcNow);

        if (!withdrawResult.IsSuccessful)
        {
            return Result.Fail(withdrawResult.Error);
        }

        // Deposit to destination wallet
        var depositResult = destinationWallet.Deposit(
            new Money(request.Amount, destinationWallet.TotalAmount.Currency),
            $"Transfer from {sourceWallet.Name}",
            request.TransferedOn,
            dateTimeService.UtcNow);

        if (!depositResult.IsSuccessful)
        {
            return Result.Fail(depositResult.Error);
        }

        dbContext.Wallets.Update(sourceWallet);
        dbContext.Wallets.Update(destinationWallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}