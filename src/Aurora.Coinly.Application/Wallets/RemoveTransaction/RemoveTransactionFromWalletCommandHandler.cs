using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.RemoveTransaction;

internal sealed class RemoveTransactionFromWalletCommandHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository) : ICommandHandler<RemoveTransactionFromWalletCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionFromWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId, transaction.UserId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Remove transaction from wallet
        var result = wallet.RemoveTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}