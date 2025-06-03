using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Transactions.GetById;

internal sealed class GetTransactionByIdQueryHandler(
    ITransactionRepository transactionRepository) : IQueryHandler<GetTransactionByIdQuery, TransactionModel>
{
    public async Task<Result<TransactionModel>> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null)
        {
            return Result.Fail<TransactionModel>(TransactionErrors.NotFound);
        }

        // Return transaction model
        return transaction.ToModel(DisplayDateType.TransactionDate);
    }
}