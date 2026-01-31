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
        var query = dbContext
            .Transactions
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.UserId == userContext.UserId)
            .AsQueryable();

        // Project to transaction models
        TransactionModel transaction = await query
            .Include(x => x.Category)
            .Select(x => new TransactionModel(
                x.Id,
                x.Description,
                x.TransactionDate,
                x.MaxPaymentDate,
                x.PaymentDate,
                x.TransactionDate,
                DisplayDateType.TransactionDate,
                x.Amount.Currency.Code,
                x.Amount.Amount,
                x.Type,
                x.Status,
                x.IsPaid,
                new TransactionCategoryModel(
                    x.Category.Id,
                    x.Category.Name,
                    x.Category.Type,
                    x.Category.Color.Value,
                    x.Category.Group),
                x.PaymentMethod == null
                    ? null
                    : new TransactionPaymentMethodModel(
                        x.PaymentMethod.Id,
                        x.PaymentMethod.Name),
                x.Wallet == null
                    ? null
                    : new TransactionWalletModel(
                        x.Wallet.Id,
                        x.Wallet.Name,
                        x.Wallet.Type,
                        x.Wallet.Color.Value),
                x.Notes,
                x.IsRecurring,
                x.InstallmentNumber))
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction is null)
        {
            return Result.Fail<TransactionModel>(TransactionErrors.NotFound);
        }

        // Return transaction model
        return transaction;
    }
}