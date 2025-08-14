using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Infrastructure.Methods;

internal sealed class PaymentMethodRepository(
    ApplicationDbContext dbContext) : BaseRepository<PaymentMethod>(dbContext), IPaymentMethodRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, Guid userId) => await dbContext
        .PaymentMethods
        .Include(x => x.Wallet)
        .Where(x => x.Id == id && x.UserId == userId)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<PaymentMethod>> GetListAsync(Guid userId, bool showDeleted)
    {
        var query = dbContext
           .PaymentMethods
           .Include(x => x.Wallet)
           .Where(x => x.UserId == userId)
           .AsNoTracking()
           .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}