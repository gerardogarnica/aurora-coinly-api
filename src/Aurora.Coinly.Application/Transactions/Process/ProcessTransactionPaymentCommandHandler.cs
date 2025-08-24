using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class ProcessTransactionPaymentCommandHandler(
    ICoinlyDbContext dbContext,
    ITransactionRepository transactionRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<ProcessTransactionPaymentCommand>
{
    public async Task<Result> Handle(
        ProcessTransactionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var transactionId in request.TransactionIds)
        {
            // Get transaction
            var transaction = await transactionRepository.GetByIdAsync(transactionId, userContext.UserId);
            if (transaction is null)
            {
                return Result.Fail(TransactionErrors.NotFound);
            }

            // Get wallet
            Wallet? wallet = await dbContext
                .Wallets
                .Include(x => x.Methods)
                .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

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
        }

        return Result.Ok();
    }
}