using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.Remove;

internal sealed class RemoveTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IDateTimeService dateTimeService) : ICommandHandler<RemoveTransactionCommand>
{
    public async Task<Result> Handle(
        RemoveTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Remove transaction
        var result = transaction.Remove(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        transactionRepository.Update(transaction);

        return Result.Ok();
    }
}