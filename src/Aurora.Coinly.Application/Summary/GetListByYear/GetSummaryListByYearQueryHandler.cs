namespace Aurora.Coinly.Application.Summary.GetListByYear;

internal sealed class GetSummaryListByYearQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetSummaryListByYearQuery, IReadOnlyCollection<MonthlySummaryModel>>
{
    public async Task<Result<IReadOnlyCollection<MonthlySummaryModel>>> Handle(
        GetSummaryListByYearQuery request,
        CancellationToken cancellationToken)
    {
        // Get summaries
        List<MonthlySummary> summaries = await dbContext
            .MonthlySummaries
            .Where(x => x.UserId == userContext.UserId && x.Year == request.Year && x.CurrencyCode == request.CurrencyCode)
            .AsNoTracking()
            .AsQueryable()
            .OrderBy(x => x.Month)
            .ToListAsync(cancellationToken);

        // Return summary models
        return summaries.Select(x => x.ToModel()).ToList();
    }
}