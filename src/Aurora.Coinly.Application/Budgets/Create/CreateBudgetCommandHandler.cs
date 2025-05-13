using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Budgets.Create;

internal sealed class CreateBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    IDateTimeService dateTimeService,
    BudgetPeriodService budgetPeriodService) : ICommandHandler<CreateBudgetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
        {
            return Result.Fail<Guid>(CategoryErrors.NotFound);
        }

        // Check if budget already exists
        var existingBudget = await budgetRepository.GetByCategoryIdAsync(
            request.CategoryId,
            request.Year);

        if (existingBudget is not null)
        {
            return Result.Fail<Guid>(BudgetErrors.BudgetAlreadyExists);
        }

        // Create budget
        var budget = Budget.Create(
            category,
            request.Year,
            request.Frequency,
            new Money(request.AmountLimit, Currency.FromCode(request.CurrencyCode)),
            dateTimeService.UtcNow,
            budgetPeriodService);

        await budgetRepository.AddAsync(budget, cancellationToken);

        return budget.Id;
    }
}