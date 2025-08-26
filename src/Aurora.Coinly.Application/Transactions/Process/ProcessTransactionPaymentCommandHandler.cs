namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class ProcessTransactionPaymentCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<ProcessTransactionPaymentCommand>
{
    public async Task<Result> Handle(
        ProcessTransactionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var transactionId in request.TransactionIds)
        {
            // Get transaction
            Transaction? transaction = await dbContext
                .Transactions
                .Include(x => x.Category)
                .SingleOrDefaultAsync(x => x.Id == transactionId && x.UserId == userContext.UserId, cancellationToken);

            if (transaction is null)
            {
                return Result.Fail(TransactionErrors.NotFound);
            }

            // Get wallet
            Wallet? wallet = await dbContext
                .Wallets
                .Include(x => x.Methods)
                .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

            if (wallet is null)
            {
                return Result.Fail(WalletErrors.NotFound);
            }

            // Pay transaction
            var result = transaction.Pay(
                wallet,
                request.PaymentDate,
                dateTimeService.UtcNow);

            if (!result.IsSuccessful)
            {
                return Result.Fail(result.Error);
            }

            dbContext.Transactions.Update(transaction);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}