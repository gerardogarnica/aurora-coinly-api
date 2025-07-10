namespace Aurora.Coinly.Domain.Users;

public sealed class UserCreatedEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
}