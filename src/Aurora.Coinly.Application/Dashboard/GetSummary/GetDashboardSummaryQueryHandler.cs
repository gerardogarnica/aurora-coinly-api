using Aurora.Coinly.Application.Transactions;
using System.Threading.Tasks;

namespace Aurora.Coinly.Application.Dashboard.GetSummary;

internal sealed class GetDashboardSummaryQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryModel>
{
    private const string CurrencyCode = "USD";
    private const int MonthsToShow = 12;
    private const int MaxNumberOfCategories = 10;
    private const int MaxNumberOfTransactions = 10;

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

        // Get recent transactions
        List<TransactionModel> recentTransactions = [.. (await GetRecentTransactions(cancellationToken))
            .Select(x => x.ToModel(DisplayDateType.TransactionDate))];

        // Get upcoming pending payments
        List<TransactionModel> upcomingPayments = [.. (await GetUpcomingPendingPayments(cancellationToken))
            .Select(x => x.ToModel(DisplayDateType.TransactionDate))];

        // Create and return the dashboard summary model
        DashboardSummaryModel dashboardSummaryModel = new(
            CurrencyCode,
            totalBalance,
            GetSummaryCard(currentPeriod.TotalIncome, previousPeriod.TotalIncome),
            GetSummaryCard(currentPeriod.TotalExpense, previousPeriod.TotalExpense),
            GetSummaryCard(currentPeriod.Savings, previousPeriod.Savings),
            monthlyTrends,
            categoryExpenses,
            recentTransactions,
            upcomingPayments);

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

    private async Task<List<Transaction>> GetRecentTransactions(CancellationToken cancellationToken) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .Include(x => x.PaymentMethod)
        .Include(x => x.Wallet)
        .AsSplitQuery()
        .Where(x => x.UserId == userContext.UserId)
        .AsNoTracking()
        .OrderByDescending(x => x.TransactionDate)
        .ThenByDescending(x => x.CreatedOnUtc)
        .Take(MaxNumberOfTransactions)
        .ToListAsync(cancellationToken);

    private async Task<List<Transaction>> GetUpcomingPendingPayments(CancellationToken cancellationToken) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .Include(x => x.PaymentMethod)
        .Include(x => x.Wallet)
        .AsSplitQuery()
        .Where(x => x.UserId == userContext.UserId && x.Status == TransactionStatus.Pending)
        .AsNoTracking()
        .OrderBy(x => x.MaxPaymentDate)
        .ThenBy(x => x.TransactionDate)
        .Take(MaxNumberOfTransactions)
        .ToListAsync(cancellationToken);
}