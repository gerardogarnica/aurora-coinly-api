namespace Aurora.Coinly.Application.Transactions.Remove;

internal sealed class RemoveTransactionCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<RemoveTransactionCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        Transaction? transaction = await dbContext
            .Transactions
            .Include(x => x.Category)
            .Include(x => x.PaymentMethod)
            .Include(x => x.Wallet)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Remove transaction
        var result = transaction.Remove(dateTimeService.UtcNow, dateTimeService.Today);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Transactions.Update(transaction);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}