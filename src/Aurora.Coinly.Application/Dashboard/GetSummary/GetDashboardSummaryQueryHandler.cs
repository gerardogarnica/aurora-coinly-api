namespace Aurora.Coinly.Application.Dashboard.GetSummary;

internal sealed class GetDashboardSummaryQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryModel>
{
    private const string CurrencyCode = "USD";
    private const int MonthsToShow = 12;
    private const int MaxNumberOfCategories = 10;
    private const int MaxNumberOfTransactions = 5;

    public async Task<Result<DashboardSummaryModel>> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // Get the current date
        DateOnly currentDate = dateTimeService.Today;
        DateOnly previousDate = currentDate.AddMonths(-1);

        // Get the current and previous month summaries
        MonthlySummary currentPeriod = await GetMonthlySummary(currentDate, cancellationToken);
        MonthlySummary previousPeriod = await GetMonthlySummary(previousDate, cancellationToken);

        // Get the total balance
        var totalBalance = await GetCurrentBalance(cancellationToken);

        // Get the last MonthsToShow summaries
        List<MonthlySummary> summaries = await GetSummaries(cancellationToken);

        List<MonthlyTrend> monthlyTrends = [.. summaries.Select(x => new MonthlyTrend(
            x.Year,
            x.Month,
            x.TotalIncome,
            x.TotalExpense,
            x.Savings))];

        // Get expenses by category for the current month
        List<CategoryExpense> categoryExpenses = await GetExpensesByCategory(currentDate.Year, currentDate.Month, cancellationToken);

        // Get expenses by group for the current month
        List<CategoryGroupExpense> groupExpenses = await GetExpensesByGroup(currentDate.Year, currentDate.Month, cancellationToken);

        // Get recent transactions
        List<DashboardTransactionModel> recentTransactions = await GetRecentTransactions(cancellationToken);

        // Get upcoming pending payments
        List<DashboardTransactionModel> upcomingPayments = await GetUpcomingPendingPayments(cancellationToken);

        // Get wallets
        List<DashboardWalletModel> wallets = await GetActiveWallets(cancellationToken);

        // Create and return the dashboard summary model
        DashboardSummaryModel dashboardSummaryModel = new(
            CurrencyCode,
            totalBalance,
            GetSummaryCard(currentPeriod.TotalIncome, previousPeriod.TotalIncome),
            GetSummaryCard(currentPeriod.TotalExpense, previousPeriod.TotalExpense),
            GetSummaryCard(currentPeriod.Savings, previousPeriod.Savings),
            monthlyTrends,
            categoryExpenses,
            groupExpenses,
            recentTransactions,
            upcomingPayments,
            wallets);

        return dashboardSummaryModel;
    }

    private async Task<decimal> GetCurrentBalance(CancellationToken cancellationToken)
    {
        List<Wallet> wallets = await dbContext
            .Wallets
            .Where(x => x.UserId == userContext.UserId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return wallets.Sum(x => x.TotalAmount.Amount);
    }

    private async Task<MonthlySummary> GetMonthlySummary(DateOnly date, CancellationToken cancellationToken)
    {
        MonthlySummary summary = await dbContext
            .MonthlySummaries
            .Where(x =>
                x.UserId == userContext.UserId &&
                x.Year == date.Year &&
                x.Month == date.Month &&
                x.CurrencyCode == CurrencyCode)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return summary
            ?? MonthlySummary
                .Create(userContext.UserId, date.Year, CurrencyCode, decimal.Zero)
                .ToList()
                .Find(x => x.Month == date.Month);
    }

    private SummaryCard GetSummaryCard(decimal currentAmount, decimal previousAmount)
    {
        decimal difference = currentAmount - previousAmount;
        decimal percentageChange = previousAmount == 0 ? 0 : (difference / previousAmount) * 100;
        return new SummaryCard(currentAmount, Math.Round(percentageChange, 2));
    }

    private List<int> GetLastMonths(int year, int month)
    {
        var periods = new List<int>();
        for (int i = MonthsToShow - 1; i >= 0; i--)
        {
            DateOnly date = new DateOnly(year, month, 1).AddMonths(-i);
            periods.Add(date.Year * 100 + date.Month);
        }

        return periods;
    }

    private async Task<List<MonthlySummary>> GetSummaries(CancellationToken cancellationToken)
    {
        List<int> periods = GetLastMonths(dateTimeService.UtcNow.Year, dateTimeService.UtcNow.Month);

        return await dbContext
            .MonthlySummaries
            .Where(x =>
                x.UserId == userContext.UserId &&
                x.CurrencyCode == CurrencyCode &&
                periods.Contains(x.Year * 100 + x.Month))
            .AsNoTracking()
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CategoryExpense>> GetExpensesByCategory(int year, int month, CancellationToken cancellationToken)
    {
        List<Transaction> transactions = await dbContext
            .Transactions
            .Include(x => x.Category)
            .Where(x =>
                x.Category.Type == TransactionType.Expense &&
                x.UserId == userContext.UserId &&
                x.TransactionDate.Year == year &&
                x.TransactionDate.Month == month)
            .ToListAsync(cancellationToken);

        List<CategoryExpense> categories = [.. transactions
            .GroupBy(x => x.Category.Name)
            .Select(g => new CategoryExpense(g.Key, g.Sum(x => x.Amount.Amount)))
            .OrderByDescending(x => x.Amount)];

        if (categories.Count <= MaxNumberOfCategories)
        {
            return categories;
        }

        List<CategoryExpense> topCategories = [.. categories.Take(MaxNumberOfCategories - 1)];
        decimal othersAmount = categories.Skip(MaxNumberOfCategories - 1).Sum(x => x.Amount);
        topCategories.Add(new CategoryExpense("Others", othersAmount));

        return topCategories;
    }

    private async Task<List<CategoryGroupExpense>> GetExpensesByGroup(int year, int month, CancellationToken cancellationToken)
    {
        List<Transaction> transactions = await dbContext
            .Transactions
            .Include(x => x.Category)
            .Where(x =>
                x.Category.Type == TransactionType.Expense &&
                x.UserId == userContext.UserId &&
                x.TransactionDate.Year == year &&
                x.TransactionDate.Month == month)
            .ToListAsync(cancellationToken);

        List<CategoryGroupExpense> groups = [.. transactions
            .GroupBy(x => x.Category.Group)
            .Select(g => new CategoryGroupExpense(g.Key.ToString(), g.Sum(x => x.Amount.Amount)))
            .OrderByDescending(x => x.Amount)];

        if (groups.Count <= MaxNumberOfCategories)
        {
            return groups;
        }

        List<CategoryGroupExpense> topGroups = [.. groups.Take(MaxNumberOfCategories - 1)];
        decimal othersAmount = groups.Skip(MaxNumberOfCategories - 1).Sum(x => x.Amount);
        topGroups.Add(new CategoryGroupExpense("Others", othersAmount));

        return topGroups;
    }

    private async Task<List<DashboardTransactionModel>> GetRecentTransactions(CancellationToken cancellationToken) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .AsNoTracking()
        .Where(x => x.UserId == userContext.UserId)
        .OrderByDescending(x => x.TransactionDate)
        .ThenByDescending(x => x.CreatedOnUtc)
        .Take(MaxNumberOfTransactions)
        .Select(x => new DashboardTransactionModel(
            x.Id,
            x.Description,
            x.TransactionDate,
            x.MaxPaymentDate,
            x.PaymentMethod == null ? string.Empty : x.PaymentMethod.Name,
            x.Amount.Currency.Code,
            x.Amount.Amount,
            x.Type,
            x.Status,
            new DashboardTransactionCategoryModel(
                x.Category.Id,
                x.Category.Name,
                x.Category.Type,
                x.Category.Color.Value,
                x.Category.Group)))
        .ToListAsync(cancellationToken);

    private async Task<List<DashboardTransactionModel>> GetUpcomingPendingPayments(CancellationToken cancellationToken) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .AsNoTracking()
        .Where(x => x.UserId == userContext.UserId && x.Status == TransactionStatus.Pending)
        .OrderBy(x => x.MaxPaymentDate)
        .ThenByDescending(x => x.TransactionDate)
        .Take(MaxNumberOfTransactions)
        .Select(x => new DashboardTransactionModel(
            x.Id,
            x.Description,
            x.TransactionDate,
            x.MaxPaymentDate,
            x.PaymentMethod == null ? string.Empty : x.PaymentMethod.Name,
            x.Amount.Currency.Code,
            x.Amount.Amount,
            x.Type,
            x.Status,
            new DashboardTransactionCategoryModel(
                x.Category.Id,
                x.Category.Name,
                x.Category.Type,
                x.Category.Color.Value,
                x.Category.Group)))
        .ToListAsync(cancellationToken);

    private async Task<List<DashboardWalletModel>> GetActiveWallets(CancellationToken cancellationToken) => await dbContext
        .Wallets
        .AsNoTracking()
        .Where(x => x.UserId == userContext.UserId && !x.IsDeleted)
        .OrderBy(x => x.Name)
        .Select(x => new DashboardWalletModel(
            x.Id,
            x.Name,
            x.TotalAmount.Currency.Code,
            x.AvailableAmount.Amount,
            x.SavingsAmount.Amount,
            x.TotalAmount.Amount,
            x.Color.Value))
        .ToListAsync(cancellationToken);
}