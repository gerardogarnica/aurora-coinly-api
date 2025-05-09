﻿using Aurora.Coinly.Domain.Summary;

namespace Aurora.Coinly.Application.Summary.GetListByYear;

internal sealed class GetSummaryListByYearQueryHandler(
    IMonthlySummaryRepository summaryRepository) : IQueryHandler<GetSummaryListByYearQuery, IReadOnlyCollection<MonthlySummaryModel>>
{
    public async Task<Result<IReadOnlyCollection<MonthlySummaryModel>>> Handle(
        GetSummaryListByYearQuery request,
        CancellationToken cancellationToken)
    {
        // Get summaries
        var summaries = await summaryRepository.GetListAsync(request.Year, request.CurrencyCode);

        // Return summary models
        return summaries.Select(x => x.ToModel()).ToList();
    }
}