namespace Aurora.Coinly.Infrastructure.Time;

internal sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
