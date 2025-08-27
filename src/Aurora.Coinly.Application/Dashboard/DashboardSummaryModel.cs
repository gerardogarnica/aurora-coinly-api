using Aurora.Coinly.Application.Transactions;

namespace Aurora.Coinly.Application.Dashboard;

public sealed record DashboardSummaryModel(
    string Currency,
    decimal TotalBalance,
    SummaryCard TotalIncome,
    SummaryCard TotalExpenses,
    SummaryCard TotalSavings,
    List<MonthlyTrend> MonthlyTrends,
    List<CategoryExpense> ExpensesByCategory,
    List<TransactionModel> RecentTransactions,
    List<TransactionModel> UpcomingPayments);

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