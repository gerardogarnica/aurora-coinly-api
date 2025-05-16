namespace Aurora.Coinly.Application.Budgets.Create;

internal sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 1)
            .WithMessage("'{PropertyName}' must be between {From} and {To}.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.AmountLimit)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(9, 2, true);
    }

    private static bool BeAValidCurrencyCode(string currencyCode)
    {
        return Currency.All.Contains(Currency.FromCode(currencyCode));
    }
}