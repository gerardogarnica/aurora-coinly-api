using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Budgets.Create;

internal sealed class CreateBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CreateBudgetCommand, Guid>
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

        // Create budget
        var budget = Budget.Create(
            category,
            new Money(request.AmountLimit, Currency.FromCode(request.CurrencyCode)),
            DateRange.Create(request.PeriodBegins, request.PeriodEnds),
            request.Notes,
            dateTimeService.UtcNow);

        await budgetRepository.AddAsync(budget, cancellationToken);

        return budget.Id;
    }
}