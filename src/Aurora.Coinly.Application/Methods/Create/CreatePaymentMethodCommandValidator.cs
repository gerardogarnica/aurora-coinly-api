namespace Aurora.Coinly.Application.Methods.Create;

internal sealed class CreatePaymentMethodCommandValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.MaxDaysToReverse)
            .NotEmpty()
            .InclusiveBetween(0, 15);

        RuleFor(x => x.SuggestedPaymentDay)
            .Null()
            .When(x => x.AutoMarkAsPaid);

        RuleFor(x => x.SuggestedPaymentDay)
            .NotNull()
            .InclusiveBetween(1, 31)
            .When(x => !x.AutoMarkAsPaid);

        RuleFor(x => x.StatementCutoffDay)
            .Null()
            .When(x => x.AutoMarkAsPaid);

        RuleFor(x => x.StatementCutoffDay)
            .NotNull()
            .InclusiveBetween(1, 31)
            .When(x => !x.AutoMarkAsPaid);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}