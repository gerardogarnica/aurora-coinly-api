namespace Aurora.Coinly.Domain.Shared;

public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero() => Zero(Currency.Default);
    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);

    public static Money operator +(Money first, Money second)
    {
        ValidateCurrencies(first, second);

        return new Money(first.Amount + second.Amount, first.Currency);
    }

    public static Money operator -(Money first, Money second)
    {
        ValidateCurrencies(first, second);

        return new Money(first.Amount - second.Amount, first.Currency);
    }

    public static bool operator <(Money first, Money second)
    {
        return CompareAmounts(first, second) < 0;
    }

    public static bool operator >(Money first, Money second)
    {
        return CompareAmounts(first, second) > 0;
    }

    private static void ValidateCurrencies(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }
    }

    private static int CompareAmounts(Money first, Money second)
    {
        ValidateCurrencies(first, second);

        return first.Amount.CompareTo(second.Amount);
    }
}
