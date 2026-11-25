namespace Aurora.Coinly.Domain.Budgets;

public class BudgetPeriodService
{
    public IEnumerable<DateRange> GeneratePeriods(BudgetFrequency frequency, int year)
    {
        return frequency switch
        {
            BudgetFrequency.Weekly => Enumerable.Range(0, 52)
                .Select(week =>
                {
                    var start = new DateOnly(year, 1, 1).AddDays(week * 7);
                    var end = new DateOnly(year, 1, 1).AddDays((week + 1) * 7 - 1);

                    return DateRange.Create(start, end).Value;
                }),

            BudgetFrequency.Biweekly => Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var start = new DateOnly(year, month, 1);
                    var end = new DateOnly(year, month, 15);

                    return DateRange.Create(start, end).Value;
                })
                .Union(Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var start = new DateOnly(year, month, 16);
                    var end = start.AddMonths(1).AddDays(-1);

                    return DateRange.Create(start, end).Value;
                })),

            BudgetFrequency.Monthly => Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var start = new DateOnly(year, month, 1);
                    var end = start.AddMonths(1).AddDays(-1);

                    return DateRange.Create(start, end).Value;
                }),

            BudgetFrequency.Quarterly => Enumerable.Range(1, 4)
                .Select(quarter =>
                {
                    var start = new DateOnly(year, (quarter * 3) -2 , 1);
                    var end = start.AddMonths(3).AddDays(-1);

                    return DateRange.Create(start, end).Value;
                }),

            BudgetFrequency.SemiAnnual =>
                [
                    DateRange.Create(new DateOnly(year, 1, 1), new DateOnly(year, 6, 30)).Value,
                    DateRange.Create(new DateOnly(year, 7, 1), new DateOnly(year, 12, 31)).Value
                ],

            BudgetFrequency.Annual =>
                [
                    DateRange.Create(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31)).Value
                ],

            _ => throw new NotImplementedException($"Budget frequency {frequency} is not implemented."),
        };
    }
}