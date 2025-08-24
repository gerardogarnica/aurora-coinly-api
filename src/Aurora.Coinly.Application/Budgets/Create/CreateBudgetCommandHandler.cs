namespace Aurora.Coinly.Application.Budgets.Create;

internal sealed class CreateBudgetCommandHandler(
    ICoinlyDbContext dbContext,
    IBudgetRepository budgetRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService,
    BudgetPeriodService budgetPeriodService) : ICommandHandler<CreateBudgetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        Category? category = await dbContext
            .Categories
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userContext.UserId, cancellationToken);

        if (category is null)
        {
            return Result.Fail<Guid>(CategoryErrors.NotFound);
        }

        // Check if budget already exists
        var existingBudget = await budgetRepository.GetByCategoryIdAsync(
            userContext.UserId,
            request.CategoryId,
            request.Year);

        if (existingBudget is not null)
        {
            return Result.Fail<Guid>(BudgetErrors.BudgetAlreadyExists);
        }

        // Create budget
        var budget = Budget.Create(
            userContext.UserId,
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