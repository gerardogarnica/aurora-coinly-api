namespace Aurora.Coinly.Application.Budgets.GetById;

internal sealed class GetBudgetByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetBudgetByIdQuery, BudgetModel>
{
    public async Task<Result<BudgetModel>> Handle(
        GetBudgetByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get budget
        Budget? budget = await dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .ThenInclude(x => x.Transactions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (budget is null)
        {
            return Result.Fail<BudgetModel>(BudgetErrors.NotFound);
        }

        // Return budget model
        return budget.ToModel();
    }
}