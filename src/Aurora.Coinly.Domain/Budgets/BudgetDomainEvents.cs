namespace Aurora.Coinly.Domain.Budgets;

public sealed class BudgetUpdatedEvent(Budget budget) : DomainEvent
{
    public Budget Budget { get; init; } = budget;
}