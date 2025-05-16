namespace Aurora.Coinly.Application.Budgets.UpdateLimit;

internal sealed class UpdateBudgetLimitCommandValidator : AbstractValidator<UpdateBudgetLimitCommand>
{
    public UpdateBudgetLimitCommandValidator()
    {
        RuleFor(x => x.AmountLimit)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(9, 2, true);
    }
}