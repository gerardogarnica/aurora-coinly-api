using Aurora.Coinly.Application.Summary.UpdateSavings;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

internal sealed class UpdateSummaryOnSavingsAssignedEventHandler(
    ISender sender) : IDomainEventHandler<WalletSavingsUpdatedEvent>
{
    public async Task Handle(
        WalletSavingsUpdatedEvent notification, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateSummarySavingsCommand(
            notification.WalletId,
            notification.Amount,
            notification.AssignedOn,
            notification.IsIncrement);

        Result result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(UpdateSummaryOnSavingsAssignedEventHandler), result.Error);
        }
    }
}