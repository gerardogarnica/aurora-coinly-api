using Aurora.Coinly.Application.Summary.AddTransaction;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateSummaryOnTransactionPaidEventHandler(
    ICommandHandler<AddSummaryTransactionCommand> handler) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddSummaryTransactionCommand(notification.TransactionId);

        Result result = await handler.Handle(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddSummaryTransactionCommand), result.Error);
        }
    }
}