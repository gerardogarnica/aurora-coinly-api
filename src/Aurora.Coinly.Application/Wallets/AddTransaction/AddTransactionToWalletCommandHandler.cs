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

        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(transaction.Wallet!.Id);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Deposit or withdrawal
        var result = transaction.Type == TransactionType.Income
            ? wallet.Deposit(transaction, dateTimeService.UtcNow)
            : wallet.Withdraw(transaction, dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}