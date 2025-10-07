namespace Aurora.Coinly.Domain.UnitTests.Abstractions;

public abstract class BaseTest
{
    public static T AssertDomainEventWasPublished<T>(BaseEntity entity)
    {
        var domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault();

        if (domainEvent is null)
        {
            throw new Exception($"Expected domain event of type {typeof(T).Name} was not published.");
        }

        return domainEvent;
    }
}