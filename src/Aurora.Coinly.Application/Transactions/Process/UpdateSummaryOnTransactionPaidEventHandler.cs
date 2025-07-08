using Aurora.Coinly.Application.Summary.AddTransaction;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateSummaryOnTransactionPaidEventHandler(
    ISender sender) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddSummaryTransactionCommand(notification.TransactionId);

        Result result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddSummaryTransactionCommand), result.Error);
        }
    }
}