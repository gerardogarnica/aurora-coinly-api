using MediatR;

namespace Aurora.Coinly.Domain.Abstractions;

public interface IDomainEvent: INotification
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}