using Aurora.Coinly.Application.Wallets.AddTransaction;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateWalletOnTransactionPaidEventHandler(
    ISender sender) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddTransactionToWalletCommand(notification.TransactionId);

        Result result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddTransactionToWalletCommand), result.Error);
        }
    }
}