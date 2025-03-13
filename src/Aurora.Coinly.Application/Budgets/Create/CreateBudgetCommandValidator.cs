namespace Aurora.Coinly.Application.Budgets.Create;

internal sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.AmountLimit)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(2, 9, true);

        RuleFor(x => x.PeriodEnds)
            .GreaterThanOrEqualTo(x => x.PeriodBegins);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }

    private static bool BeAValidCurrencyCode(string currencyCode)
    {
        return Currency.All.Contains(Currency.FromCode(currencyCode));
    }
}