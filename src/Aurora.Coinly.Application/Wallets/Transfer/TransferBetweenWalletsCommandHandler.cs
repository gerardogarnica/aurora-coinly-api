using Aurora.Coinly.Domain.Wallets;
using System.Transactions;

namespace Aurora.Coinly.Application.Wallets.Transfer;

internal sealed class TransferBetweenWalletsCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<TransferBetweenWalletsCommand>
{
    public async Task<Result> Handle(
        TransferBetweenWalletsCommand request,
        CancellationToken cancellationToken)
    {
        // Get source wallet
        var sourceWallet = await walletRepository.GetByIdAsync(request.SourceWalletId);
        if (sourceWallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Get destination wallet
        var destinationWallet = await walletRepository.GetByIdAsync(request.DestinationWalletId);
        if (destinationWallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Check if the currency wallets match
        if (sourceWallet.TotalAmount.Currency != destinationWallet.TotalAmount.Currency)
        {
            return Result.Fail(WalletErrors.CurrenciesNotMatch);
        }

        using (var transactionScope = new TransactionScope())
        {
            // Withdraw from source wallet
            var withdrawResult = sourceWallet.Withdraw(
                new Money(request.Amount, sourceWallet.TotalAmount.Currency),
                $"Transfer to {destinationWallet.Name}",
                request.TransferedOn,
                dateTimeService.UtcNow);

            if (!withdrawResult.IsSuccessful)
            {
                return Result.Fail(withdrawResult.Error);
            }

            // Deposit to destination wallet
            var depositResult = destinationWallet.Deposit(
                new Money(request.Amount, destinationWallet.TotalAmount.Currency),
                $"Transfer from {sourceWallet.Name}",
                request.TransferedOn,
                dateTimeService.UtcNow);

            if (!depositResult.IsSuccessful)
            {
                return Result.Fail(depositResult.Error);
            }

            walletRepository.Update(sourceWallet);
            walletRepository.Update(destinationWallet);

            transactionScope.Complete();
        }

        return Result.Ok();
    }
}