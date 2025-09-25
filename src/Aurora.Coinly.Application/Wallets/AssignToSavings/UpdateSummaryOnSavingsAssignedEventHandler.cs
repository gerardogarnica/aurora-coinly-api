using Aurora.Coinly.Application.Summary.UpdateSavings;

namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

internal sealed class UpdateSummaryOnSavingsAssignedEventHandler(
    ICommandHandler<UpdateSummarySavingsCommand> handler) : IDomainEventHandler<WalletSavingsUpdatedEvent>
{
    public async Task Handle(
        WalletSavingsUpdatedEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSummarySavingsCommand(
            notification.WalletId,
            notification.UserId,
            notification.Amount,
            notification.AssignedOn,
            notification.IsIncrement);

        Result result = await handler.Handle(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(UpdateSummaryOnSavingsAssignedEventHandler), result.Error);
        }
    }
}