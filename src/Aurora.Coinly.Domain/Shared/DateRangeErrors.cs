namespace Aurora.Coinly.Domain.Shared;

public static class DateRangeErrors
{
    public static readonly BaseError StartAfterEnd = BaseError.Validation(
        "DateRange.StartAfterEnd",
        "The start date must be on or before the end date");
}
