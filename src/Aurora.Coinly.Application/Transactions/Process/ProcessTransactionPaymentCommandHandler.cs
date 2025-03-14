using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class ProcessTransactionPaymentCommandHandler(
    ITransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<ProcessTransactionPaymentCommand>
{
    public async Task<Result> Handle(
        ProcessTransactionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Pay transaction
        var result = transaction.Pay(
            wallet,
            request.PaymentDate,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        transactionRepository.Update(transaction);

        return Result.Ok();
    }
}