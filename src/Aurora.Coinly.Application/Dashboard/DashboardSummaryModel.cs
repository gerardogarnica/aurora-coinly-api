using Aurora.Coinly.Application.Transactions;
using Aurora.Coinly.Application.Wallets;

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
    List<TransactionModel> RecentTransactions,
    List<TransactionModel> UpcomingPayments,
    List<WalletModel> Wallets);

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