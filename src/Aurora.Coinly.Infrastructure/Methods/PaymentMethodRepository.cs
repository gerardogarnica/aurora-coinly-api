using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Infrastructure.Methods;

internal sealed class PaymentMethodRepository(
    ApplicationDbContext dbContext) : BaseRepository<PaymentMethod>(dbContext), IPaymentMethodRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<PaymentMethod?> GetByIdAsync(Guid id) => await dbContext
        .PaymentMethods
        .Include(x => x.Wallet)
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<PaymentMethod>> GetListAsync(bool showDeleted)
    {
        var query = dbContext
           .PaymentMethods
           .Include(x => x.Wallet)
           .AsNoTracking()
           .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}