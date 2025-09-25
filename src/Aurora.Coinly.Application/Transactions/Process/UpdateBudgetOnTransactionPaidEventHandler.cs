using Aurora.Coinly.Application.Budgets.AddTransaction;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class UpdateBudgetOnTransactionPaidEventHandler(
    ICommandHandler<AddTransactionToBudgetCommand> handler) : IDomainEventHandler<TransactionPaidEvent>
{
    public async Task Handle(
        TransactionPaidEvent notification,
        CancellationToken cancellationToken)
    {
        var command = new AddTransactionToBudgetCommand(notification.TransactionId);

        Result result = await handler.Handle(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            throw new AuroraCoinlyException(nameof(AddTransactionToBudgetCommand), result.Error);
        }
    }
}