using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets.Update;

internal sealed class UpdateBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateBudgetCommand>
{
    public async Task<Result> Handle(
        UpdateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get budget
        var budget = await budgetRepository.GetByIdAsync(request.Id);
        if (budget is null)
        {
            return Result.Fail(BudgetErrors.NotFound);
        }

        // Update budget
        var result = budget.Update(
            new Money(request.AmountLimit, budget.AmountLimit.Currency),
            DateRange.Create(request.PeriodBegins, request.PeriodEnds),
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}