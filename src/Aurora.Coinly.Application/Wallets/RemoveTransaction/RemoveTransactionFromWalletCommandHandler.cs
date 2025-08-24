namespace Aurora.Coinly.Application.Wallets.RemoveTransaction;

internal sealed class RemoveTransactionFromWalletCommandHandler(
    ICoinlyDbContext dbContext) : ICommandHandler<RemoveTransactionFromWalletCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionFromWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        Transaction? transaction = await dbContext
            .Transactions
            .Include(x => x.Wallet)
            .SingleOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == request.WalletId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Remove transaction from wallet
        var result = wallet.RemoveTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Wallets.Update(wallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}