using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Transactions.Create;

internal sealed class CreateTransactionCommandHandler(
    ICategoryRepository categoryRepository,
    IPaymentMethodRepository paymentMethodRepository,
    ITransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CreateTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
        {
            return Result.Fail<Guid>(CategoryErrors.NotFound);
        }

        // Get payment method
        var method = await paymentMethodRepository.GetByIdAsync(request.PaymentMethodId);
        if (method is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        // Create transaction
        var result = Transaction.Create(
            request.Description,
            category,
            request.TransactionDate,
            method.AutoMarkAsPaid ? request.TransactionDate : request.MaxPaymentDate,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            method,
            request.Notes,
            method.AutoMarkAsPaid ? 0 : request.Installment,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail<Guid>(result.Error);
        }

        var transaction = result.Value;

        // Pay transaction when payment method is auto mark as paid
        if (method.AutoMarkAsPaid)
        {
            if (request.WalletId is null)
            {
                return Result.Fail<Guid>(WalletErrors.NotFound);
            }

            var wallet = await walletRepository.GetByIdAsync(request.WalletId.Value);

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

        await transactionRepository.AddAsync(transaction, cancellationToken);

        return transaction.Id;
    }
}