namespace Aurora.Coinly.Domain.Shared;

public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero() => Zero(Currency.Default);
    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);

    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return new Money(first.Amount + second.Amount, first.Currency);
    }

    public static Money operator -(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return new Money(first.Amount - second.Amount, first.Currency);
    }
}