using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Transactions.CreateIncome;

internal sealed class CreateIncomeTransactionCommandHandler(
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreateIncomeTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateIncomeTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, userContext.UserId);
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
        var wallet = await walletRepository.GetByIdAsync(request.WalletId, userContext.UserId);
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