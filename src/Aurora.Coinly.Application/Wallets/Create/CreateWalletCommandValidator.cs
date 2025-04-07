namespace Aurora.Coinly.Application.Wallets.Create;

internal sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(9, 2, true);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }

    private static bool BeAValidCurrencyCode(string currencyCode)
    {
        return Currency.All.Contains(Currency.FromCode(currencyCode));
    }
}