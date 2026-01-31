namespace Aurora.Coinly.Application.Transactions.GetList;

internal sealed class GetTransactionListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetTransactionListQuery, IReadOnlyCollection<TransactionModel>>
{
    public async Task<Result<IReadOnlyCollection<TransactionModel>>> Handle(
        GetTransactionListQuery request,
        CancellationToken cancellationToken)
    {
        // Get transactions
        DateRange dateRange = DateRange.Create(request.DateFrom, request.DateTo);

        var query = dbContext
            .Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userContext.UserId)
            .AsQueryable();

        query = request.DisplayDateType == DisplayDateType.TransactionDate
            ? query.Where(x => x.TransactionDate >= dateRange.Start && x.TransactionDate <= dateRange.End)
            : query.Where(x => x.PaymentDate! >= dateRange.Start && x.PaymentDate! <= dateRange.End);

        if (request.Status is not null)
        {
            query = query.Where(x => x.Status == request.Status);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (request.PaymentMethodId is not null)
        {
            query = query.Where(x => x.PaymentMethodId == request.PaymentMethodId.Value);
        }

        // Project to transaction models
        List<TransactionModel> transactions = await query
            .Include(x => x.Category)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.CreatedOnUtc)
            .Select(x => new TransactionModel(
                x.Id,
                x.Description,
                x.TransactionDate,
                x.MaxPaymentDate,
                x.PaymentDate,
                request.DisplayDateType == DisplayDateType.TransactionDate ? x.TransactionDate : x.PaymentDate ?? x.TransactionDate,
                request.DisplayDateType,
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
            .ToListAsync(cancellationToken);

        // Return transaction models
        return transactions;
    }
}