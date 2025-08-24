namespace Aurora.Coinly.Application.Budgets.UpdateLimit;

internal sealed class UpdateBudgetLimitCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateBudgetLimitCommand>
{
    public async Task<Result> Handle(
        UpdateBudgetLimitCommand request,
        CancellationToken cancellationToken)
    {
        // Get budget
        Budget? budget = await dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

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

        dbContext.Budgets.Update(budget);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}