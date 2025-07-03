namespace Aurora.Coinly.Application.Transactions.CreateExpense;

internal sealed class CreateExpenseTransactionCommandValidator : AbstractValidator<CreateExpenseTransactionCommand>
{
    public CreateExpenseTransactionCommandValidator(IDateTimeService dateTimeService)
    {
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.PaymentMethodId).NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .LessThanOrEqualTo(dateTimeService.Today)
            .WithMessage("'{PropertyName}' must be today or in the past.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeAValidCurrencyCode).WithMessage("'{PropertyName}' is not a valid currency code.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(9, 2, true);

        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x.WalletId)
            .NotNull()
            .When(x => x.MakePayment);
    }

    private static bool BeAValidCurrencyCode(string currencyCode)
    {
        return Currency.All.Contains(Currency.FromCode(currencyCode));
    }
}