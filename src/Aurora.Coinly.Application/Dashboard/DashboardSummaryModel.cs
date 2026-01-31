namespace Aurora.Coinly.Application.Dashboard;

public sealed record DashboardSummaryModel(
    string Currency,
    decimal TotalBalance,
    SummaryCard TotalIncome,
    SummaryCard TotalExpenses,
    SummaryCard TotalSavings,
    List<MonthlyTrend> MonthlyTrends,
    List<CategoryExpense> ExpensesByCategory,
    List<CategoryGroupExpense> ExpensesByGroup,
    List<DashboardTransactionModel> RecentTransactions,
    List<DashboardTransactionModel> UpcomingPayments,
    List<DashboardWalletModel> Wallets);

public sealed record SummaryCard(
    decimal Amount,
    decimal PercentageChange);

public sealed record MonthlyTrend(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Savings);

public sealed record CategoryExpense(
    string Category,
    decimal Amount);

public sealed record CategoryGroupExpense(
    string CategoryGroup,
    decimal Amount);

public sealed record DashboardTransactionModel(
    Guid TransactionId,
    string Description,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    string PaymentMethodName,
    string Currency,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionStatus Status,
    DashboardTransactionCategoryModel Category);

public sealed record DashboardTransactionCategoryModel(
    Guid CategoryId,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    string Color,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    CategoryGroup Group);

public sealed record DashboardWalletModel(
    Guid WalletId,
    string Name,
    string CurrencyCode,
    decimal AvailableAmount,
    decimal SavingsAmount,
    decimal TotalAmount,
    string Color);