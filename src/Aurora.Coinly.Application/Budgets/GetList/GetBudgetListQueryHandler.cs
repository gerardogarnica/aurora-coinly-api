namespace Aurora.Coinly.Application.Budgets.GetList;

internal sealed class GetBudgetListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetBudgetListQuery, IReadOnlyCollection<BudgetModel>>
{
    public async Task<Result<IReadOnlyCollection<BudgetModel>>> Handle(
        GetBudgetListQuery request,
        CancellationToken cancellationToken)
    {
        // Get budgets
        List<Budget> budgets = await dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .AsSplitQuery()
            .Where(x => x.UserId == userContext.UserId && x.Year == request.Year)
            .AsNoTracking()
            .AsQueryable()
            .OrderBy(x => x.Category.Name)
            .ToListAsync(cancellationToken);

        // Return budget models
        return budgets.Select(x => x.ToModel()).ToList();
    }
}