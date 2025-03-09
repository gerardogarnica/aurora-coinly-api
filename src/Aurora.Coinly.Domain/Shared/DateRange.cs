namespace Aurora.Coinly.Domain.Shared;

public record DateRange
{
    public DateOnly Start { get; init; }
    public DateOnly End { get; init; }
    public int LengthinDays => End.DayNumber - Start.DayNumber;

    private DateRange() { }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            throw new ArgumentException("Start date must be before end date");
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