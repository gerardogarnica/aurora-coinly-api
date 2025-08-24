namespace Aurora.Coinly.Application.Transactions.CreateIncome;

internal sealed class CreateIncomeTransactionCommandHandler(
    ICoinlyDbContext dbContext,
    ITransactionRepository transactionRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreateIncomeTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateIncomeTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        Category? category = await dbContext
            .Categories
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userContext.UserId, cancellationToken);

        if (category is null)
        {
            return Result.Fail<Guid>(CategoryErrors.NotFound);
        }

        if (category.Type is not TransactionType.Income)
        {
            return Result.Fail<Guid>(CategoryErrors.InvalidType);
        }

        // Create transaction
        var result = Transaction.Create(
            userContext.UserId,
            request.Description,
            category,
            request.TransactionDate,
            request.TransactionDate,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            null,
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail<Guid>(result.Error);
        }

        var transaction = result.Value;

        // Pay transaction
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail<Guid>(WalletErrors.NotFound);
        }

        result = transaction.Pay(wallet, request.TransactionDate, dateTimeService.UtcNow);
        if (!result.IsSuccessful)
        {
            return Result.Fail<Guid>(result.Error);
        }

        await transactionRepository.AddAsync(transaction, cancellationToken);

        return transaction.Id;
    }
}