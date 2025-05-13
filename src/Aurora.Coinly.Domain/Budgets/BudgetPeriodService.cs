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

                                    return DateRange.Create(start, end);
                                }),

            BudgetFrequency.Biweekly => Enumerable.Range(0, 26)
                    .Select(week =>
                    {
                        var start = new DateOnly(year, 1, 1).AddDays(week * 14);
                        var end = new DateOnly(year, 1, 1).AddDays((week + 1) * 14 - 1);

                        return DateRange.Create(start, end);
                    }),

            BudgetFrequency.Monthly => Enumerable.Range(1, 12)
                    .Select(month =>
                    {
                        var start = new DateOnly(year, month, 1);
                        var end = start.AddMonths(1).AddDays(-1);

                        return DateRange.Create(start, end);
                    }),

            BudgetFrequency.Quarterly => Enumerable.Range(0, 3)
                    .Select(quarter =>
                    {
                        var start = new DateOnly(year, (quarter * 3) + 1, 1);
                        var end = start.AddMonths(3).AddDays(-1);

                        return DateRange.Create(start, end);
                    }),

            BudgetFrequency.SemiAnnual => [
                    DateRange.Create(new DateOnly(year, 1, 1), new DateOnly(year, 6, 30)),
                    DateRange.Create(new DateOnly(year, 7, 1), new DateOnly(year, 12, 31))
                ],

            BudgetFrequency.Annual => [
                    DateRange.Create(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31))
                ],

            _ => throw new NotImplementedException($"Budget frequency {frequency} is not implemented."),
        };
    }
}