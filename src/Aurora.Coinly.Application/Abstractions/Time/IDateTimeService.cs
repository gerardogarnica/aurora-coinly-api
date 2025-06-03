namespace Aurora.Coinly.Application.Abstractions.Time;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}