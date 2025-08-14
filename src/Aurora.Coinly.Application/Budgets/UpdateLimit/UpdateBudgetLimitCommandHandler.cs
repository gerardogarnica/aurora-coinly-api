using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets.UpdateLimit;

internal sealed class UpdateBudgetLimitCommandHandler(
    IBudgetRepository budgetRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateBudgetLimitCommand>
{
    public async Task<Result> Handle(
        UpdateBudgetLimitCommand request,
        CancellationToken cancellationToken)
    {
        // Get budget
        var budget = await budgetRepository.GetByIdAsync(request.Id, userContext.UserId);
        if (budget is null)
        {
            return Result.Fail(BudgetErrors.NotFound);
        }

        // Update budget limit
        var result = budget.UpdateLimit(
            request.PeriodId,
            new Money(request.AmountLimit, budget.Periods.First().Limit.Currency),
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}