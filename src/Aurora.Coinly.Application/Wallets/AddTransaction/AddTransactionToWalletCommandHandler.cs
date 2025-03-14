using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.AddTransaction;

internal sealed class AddTransactionToWalletCommandHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository,
    IDateTimeService dateTimeService) : ICommandHandler<AddTransactionToWalletCommand>
{
    public async Task<Result> Handle(
        AddTransactionToWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail(TransactionErrors.NotPaid);
        }

        // Deposit or withdrawal
        var result = transaction.Type == TransactionType.Income
            ? wallet.Deposit(
                transaction.Amount,
                transaction.Description,
                transaction.PaymentDate!.Value,
                transaction.Id,
                dateTimeService.UtcNow)
            : wallet.Withdraw(
                transaction.Amount,
                transaction.Description,
                transaction.PaymentDate!.Value,
                transaction.Id,
                dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}