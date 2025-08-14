namespace Aurora.Coinly.Domain.Transactions;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Transaction>> GetListAsync(
        Guid userId,
        DateRange dateRange,
        TransactionStatus? status,
        Guid? categoryId,
        Guid? paymentMethodId,
        DisplayDateType displayDateType);
}