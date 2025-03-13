using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets.Close;

internal sealed class CloseBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CloseBudgetCommand>
{
    public async Task<Result> Handle(
        CloseBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get budget
        var budget = await budgetRepository.GetByIdAsync(request.Id);
        if (budget is null)
        {
            return Result.Fail(BudgetErrors.NotFound);
        }

        // Close budget
        var result = budget.Close(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}