namespace Aurora.Coinly.Application.Wallets.AddTransaction;

internal sealed class AddTransactionToWalletCommandHandler(
    ICoinlyDbContext dbContext,
    ITransactionRepository transactionRepository,
    IDateTimeService dateTimeService) : ICommandHandler<AddTransactionToWalletCommand>
{
    public async Task<Result> Handle(
        AddTransactionToWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail(TransactionErrors.NotPaid);
        }

        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .SingleOrDefaultAsync(x => x.Id == transaction.Wallet!.Id, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Deposit or withdrawal
        var result = transaction.Type == TransactionType.Income
            ? wallet.Deposit(transaction, dateTimeService.UtcNow)
            : wallet.Withdraw(transaction, dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Wallets.Update(wallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}