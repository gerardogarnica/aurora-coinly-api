namespace Aurora.Coinly.Application.Summary.GetListByYear;

public sealed record GetSummaryListByYearQuery(
    int Year,
    string CurrencyCode) : IQuery<IReadOnlyCollection<MonthlySummaryModel>>;