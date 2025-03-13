namespace Aurora.Coinly.Application.Budgets.Update;

internal sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.AmountLimit)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(2, 9, true);

        RuleFor(x => x.PeriodEnds)
            .GreaterThanOrEqualTo(x => x.PeriodBegins);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}