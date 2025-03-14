using Aurora.Coinly.Application.Wallets.AddTransaction;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateWalletOnTransactionPaidEventHandler(
    ISender sender) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddTransactionToWalletCommand(
            notification.Transaction.WalletId!.Value,
            notification.Transaction.Id);

        Result result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddTransactionToWalletCommand), result.Error);
        }
    }
}