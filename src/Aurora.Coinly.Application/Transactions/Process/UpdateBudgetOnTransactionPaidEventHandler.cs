using Aurora.Coinly.Application.Budgets.AddTransaction;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateBudgetOnTransactionPaidEventHandler(
    ISender sender) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddTransactionToBudgetCommand(notification.Transaction.Id);

        Result result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddTransactionToBudgetCommand), result.Error);
        }
    }
}