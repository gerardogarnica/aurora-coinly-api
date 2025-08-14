using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets.GetList;

internal sealed class GetBudgetListQueryHandler(
    IBudgetRepository budgetRepository,
    IUserContext userContext) : IQueryHandler<GetBudgetListQuery, IReadOnlyCollection<BudgetModel>>
{
    public async Task<Result<IReadOnlyCollection<BudgetModel>>> Handle(
        GetBudgetListQuery request,
        CancellationToken cancellationToken)
    {
        // Get budgets
        var budgets = await budgetRepository.GetListAsync(userContext.UserId, request.Year);

        // Return budget models
        return budgets.Select(x => x.ToModel()).ToList();
    }
}