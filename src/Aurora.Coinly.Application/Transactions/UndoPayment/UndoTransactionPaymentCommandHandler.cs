using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.UndoPayment;

internal sealed class UndoTransactionPaymentCommandHandler(
    ITransactionRepository transactionRepository,
    IDateTimeService dateTimeService) : ICommandHandler<UndoTransactionPaymentCommand>
{
    public async Task<Result> Handle(
        UndoTransactionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        // Undo payment
        var result = transaction.UndoPayment(dateTimeService.UtcNow, dateTimeService.Today);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        transactionRepository.Update(transaction);

        return Result.Ok();
    }
}