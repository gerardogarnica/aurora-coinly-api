namespace Aurora.Coinly.Application.Transactions.CreateExpense;

internal sealed class CreateExpenseTransactionCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreateExpenseTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateExpenseTransactionCommand request,
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

        if (category.Type is not TransactionType.Expense)
        {
            return Result.Fail<Guid>(CategoryErrors.InvalidType);
        }

        // Get payment method
        PaymentMethod? method = await dbContext
            .PaymentMethods
            .SingleOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.UserId == userContext.UserId, cancellationToken);

        if (method is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        if (method.IsDeleted)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.IsDeleted);
        }

        // Create transaction
        var result = Transaction.Create(
            userContext.UserId,
            request.Description,
            category,
            request.TransactionDate,
            GetMaxPaymentDate(method, request.TransactionDate),
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            method,
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail<Guid>(result.Error);
        }

        var transaction = result.Value;

        // Pay transaction when requested from the command
        if (request.MakePayment)
        {
            if (request.WalletId is null)
            {
                return Result.Fail<Guid>(WalletErrors.NotFound);
            }

            Wallet? wallet = await dbContext
                .Wallets
                .Include(x => x.Methods)
                .SingleOrDefaultAsync(x => x.Id == request.WalletId.Value && x.UserId == userContext.UserId, cancellationToken);

            if (wallet is null)
            {
                return Result.Fail<Guid>(WalletErrors.NotFound);
            }

            result = transaction.Pay(wallet, request.TransactionDate, dateTimeService.UtcNow);
            if (!result.IsSuccessful)
            {
                return Result.Fail<Guid>(result.Error);
            }
        }

        dbContext.Transactions.Add(transaction);

        await dbContext.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }

    private DateOnly GetMaxPaymentDate(PaymentMethod method, DateOnly transactionDate)
    {
        var maxPaymentDate = transactionDate;

        if (method.AutoMarkAsPaid)
        {
            return maxPaymentDate;
        }

        var suggestedPaymentDay = method.SuggestedPaymentDay ?? 1;
        var statementCutoffDay = method.StatementCutoffDay ?? 1;

        maxPaymentDate = transactionDate.Day < statementCutoffDay
            ? new DateOnly(transactionDate.Year, transactionDate.Month, suggestedPaymentDay)
            : new DateOnly(transactionDate.Year, transactionDate.Month + 1, suggestedPaymentDay);

        if (statementCutoffDay > suggestedPaymentDay)
        {
            maxPaymentDate = maxPaymentDate.AddMonths(1);
        }

        return maxPaymentDate;
    }
}