using Aurora.Coinly.Domain.Summary;

namespace Aurora.Coinly.Infrastructure.Summary;

internal sealed class MonthlySummaryRepository(
    ApplicationDbContext dbContext) : BaseRepository<MonthlySummary>(dbContext), IMonthlySummaryRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<MonthlySummary?> GetSummaryAsync(Guid userId, int year, int month, string currencyCode) => await dbContext
        .MonthlySummaries
        .Where(x => x.UserId == userId && x.Year == year && x.Month == month && x.Currency.Code == currencyCode)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<MonthlySummary>> GetListAsync(Guid userId, int year, string currencyCode) => await dbContext
        .MonthlySummaries
        .Where(x => x.UserId == userId && x.Year == year && x.Currency.Code == currencyCode)
        .AsNoTracking()
        .AsQueryable()
        .ToListAsync();
}