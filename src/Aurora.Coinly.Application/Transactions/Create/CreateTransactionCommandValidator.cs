namespace Aurora.Coinly.Application.Transactions.Create;

internal sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.PaymentMethodId).NotEmpty();

        RuleFor(x => x.MaxPaymentDate).GreaterThanOrEqualTo(x => x.TransactionDate);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(9, 2, true);

        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x.Installment).GreaterThanOrEqualTo(0);
    }

    private static bool BeAValidCurrencyCode(string currencyCode)
    {
        return Currency.All.Contains(Currency.FromCode(currencyCode));
    }
}