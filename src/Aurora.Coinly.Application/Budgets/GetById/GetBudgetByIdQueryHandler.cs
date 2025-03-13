using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets.GetById;

internal sealed class GetBudgetByIdQueryHandler(
    IBudgetRepository budgetRepository) : IQueryHandler<GetBudgetByIdQuery, BudgetModel>
{
    public async Task<Result<BudgetModel>> Handle(
        GetBudgetByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get budget
        var budget = await budgetRepository.GetByIdAsync(request.Id);
        if (budget is null)
        {
            return Result.Fail<BudgetModel>(BudgetErrors.NotFound);
        }

        // Return budget model
        return budget.ToModel();
    }
}