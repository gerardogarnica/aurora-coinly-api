namespace Aurora.Coinly.Domain.Summary;

public interface IMonthlySummaryRepository : IRepository<MonthlySummary>
{
    Task<MonthlySummary?> GetSummaryAsync(int year, int month, string currencyCode);
    Task<IEnumerable<MonthlySummary>> GetListAsync(int year, string currencyCode);
}