using Aurora.Coinly.Domain.UnitTests.Categories;
using Aurora.Coinly.Domain.UnitTests.Methods;
using Aurora.Coinly.Domain.UnitTests.Transactions;
using Aurora.Coinly.Domain.UnitTests.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Budgets;

public class BudgetTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();

        // Act
        var budget = Budget.Create(
            category,
            BudgetData.Year,
            BudgetData.Frequency,
            BudgetData.AmountLimit,
            DateTime.UtcNow,
            new BudgetPeriodService());

        // Assert
        budget.CategoryId.Should().Be(category.Id);
        budget.Year.Should().Be(BudgetData.Year);
        budget.Frequency.Should().Be(BudgetData.Frequency);
    }

    [Fact]
    public void Create_Should_GeneratePeriods()
    {
        // Arrange
        var category = CategoryData.GetCategory();

        // Act
        var budget = Budget.Create(
            category,
            BudgetData.Year,
            BudgetData.Frequency,
            BudgetData.AmountLimit,
            DateTime.UtcNow,
            new BudgetPeriodService());

        // Assert
        budget.Periods.Should().NotBeEmpty();
        budget.Periods.Should().HaveCount(12);
        budget.Periods.Should().AllSatisfy(p =>
        {
            p.Period.Should().BeOfType<DateRange>();
            p.Limit.Should().Be(BudgetData.AmountLimit);
        });
    }

    [Fact]
    public void UpdateLimit_Should_SetProperties()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());
        var updatedAmountLimit = new Money(200.0m, Currency.Usd);
        var period = budget.Periods.First();

        // Act
        var result = budget.UpdateLimit(
            period.Id,
            updatedAmountLimit,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        period.Should().NotBeNull();
        period.Limit.Should().Be(updatedAmountLimit);
        period.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_Should_Fail_WhenPeriodIdIsInvalid()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());
        var updatedAmountLimit = new Money(100.0m, Currency.Usd);

        // Act
        var result = budget.UpdateLimit(
            Guid.NewGuid(),
            updatedAmountLimit,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.PeriodNotFound);
    }

    [Fact]
    public void AssignTransaction_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        var period = budget.Periods.FirstOrDefault(x => x.Period.Contains(transaction.PaymentDate!.Value))!;
        var operationsCount = period.Transactions.Count;

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        period.Transactions.Should().HaveCount(operationsCount + 1);
    }

    [Fact]
    public void AssignTransaction_Should_Fail_WhenTransactionCategoryMismatch()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);

        var newCategory = CategoryData.GetCategory();
        var transaction = TransactionData.GetTransaction(newCategory, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.TransactionCategoryMismatch);
    }

    [Fact]
    public void AssignTransaction_Should_Fail_WhenTransactionNotPaid()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.TransactionNotPaid);
    }

    [Fact]
    public void AssignTransaction_Should_Fail_WhenTransactionPaymentDateOutOfRange()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1), DateTime.UtcNow);

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.TransactionPaymentDateOutOfRange);
    }

    [Fact]
    public void RemoveTransaction_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);

        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        budget.AssignTransaction(transaction);
        var period = budget.Periods.FirstOrDefault(x => x.Period.Contains(transaction.PaymentDate!.Value))!;

        transaction.UndoPayment();
        var operationsCount = period.Transactions.Count;

        // Act
        var result = budget.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        period.Transactions.Should().HaveCount(operationsCount - 1);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenTransactionNotBelongs()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        var newCategory = CategoryData.GetCategory();
        var newBudget = BudgetData.GetBudget(newCategory);
        var newTransaction = TransactionData.GetTransaction(newCategory, PaymentMethodData.GetPaymentMethod());
        newTransaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        budget.AssignTransaction(transaction);

        // Act
        var result = newBudget.RemoveTransaction(newTransaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.TransactionNotBelongs);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenIsAlreadyPaid()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        budget.AssignTransaction(transaction);
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = budget.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.TransactionAlreadyIsPaid);
    }
}