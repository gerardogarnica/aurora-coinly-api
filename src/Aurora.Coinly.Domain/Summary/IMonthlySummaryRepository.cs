namespace Aurora.Coinly.Domain.Summary;

public interface IMonthlySummaryRepository : IRepository<MonthlySummary>
{
    Task<MonthlySummary?> GetSummaryAsync(Guid userId, int year, int month, string currencyCode);
    Task<IEnumerable<MonthlySummary>> GetListAsync(Guid userId, int year, string currencyCode);
}