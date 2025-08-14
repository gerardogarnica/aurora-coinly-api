namespace Aurora.Coinly.Domain.Wallets;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByIdAsync(Guid id, Guid userId);
    Task<Wallet?> GetByIdAsync(Guid id, Guid userId, DateRange historyRange);
    Task<IEnumerable<Wallet>> GetListAsync(Guid userId, bool showDeleted);
}