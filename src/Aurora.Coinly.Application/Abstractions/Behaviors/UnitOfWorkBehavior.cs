using System.Transactions;

namespace Aurora.Coinly.Application.Abstractions.Behaviors;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await next(cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        transactionScope.Complete();

        return response;
    }
}