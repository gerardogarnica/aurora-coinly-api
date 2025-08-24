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

        List<Transaction> transactions = await dbContext
            .Transactions
            .Include(x => x.Category)
            .Include(x => x.PaymentMethod)
            .Include(x => x.Wallet)
            .AsSplitQuery()
            .Where(x => x.UserId == userContext.UserId)
            .Where(x => request.DisplayDateType == DisplayDateType.TransactionDate
                ? x.TransactionDate >= dateRange.Start && x.TransactionDate <= dateRange.End
                : x.PaymentDate! >= dateRange.Start && x.PaymentDate! <= dateRange.End)
            .Where(x => request.Status == null || x.Status == request.Status)
            .Where(x => request.CategoryId == null || x.CategoryId == request.CategoryId.Value)
            .Where(x => request.PaymentMethodId == null || x.PaymentMethodId == request.PaymentMethodId.Value)
            .AsNoTracking()
            .AsQueryable()
            .OrderBy(x => x.TransactionDate)
            .ToListAsync(cancellationToken);

        // Return transaction models
        return transactions.Select(x => x.ToModel(request.DisplayDateType)).ToList();
    }
}