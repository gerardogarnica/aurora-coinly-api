namespace Aurora.Coinly.Domain.Transactions;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetListAsync(
        DateRange dateRange,
        TransactionStatus? status,
        Guid? categoryId,
        Guid? paymentMethodId);
}