namespace Aurora.Coinly.Application.Transactions.GetById;

internal sealed class GetTransactionByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetTransactionByIdQuery, TransactionModel>
{
    public async Task<Result<TransactionModel>> Handle(
        GetTransactionByIdQuery request,
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
            return Result.Fail<TransactionModel>(TransactionErrors.NotFound);
        }

        // Return transaction model
        return transaction.ToModel(DisplayDateType.TransactionDate);
    }
}