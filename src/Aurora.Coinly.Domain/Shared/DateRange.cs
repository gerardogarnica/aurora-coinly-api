namespace Aurora.Coinly.Domain.Shared;

public sealed record DateRange : IValueObject
{
    public DateOnly Start { get; init; }
    public DateOnly End { get; init; }
    public int TotalDays => (End.DayNumber - Start.DayNumber) + 1;

    private DateRange() { }

    public static Result<DateRange> Create(DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            return Result.Fail<DateRange>(DateRangeErrors.StartAfterEnd);
        }

        return new DateRange
        {
            Start = start,
            End = end
        };
    }

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool IsWithinSameYear() => Start.Year == End.Year;

    public bool Overlaps(DateRange other)
    {
        return Start <= other.End && End >= other.Start;
    }
}