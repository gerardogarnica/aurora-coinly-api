namespace Aurora.Coinly.Domain.Wallets;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByIdAsync(Guid id, DateRange historyRange);
    Task<IEnumerable<Wallet>> GetListAsync(bool showDeleted);
}