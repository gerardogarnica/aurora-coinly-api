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
            BudgetData.AmountLimit,
            BudgetData.Period,
            BudgetData.Notes,
            DateTime.UtcNow);

        // Assert
        budget.CategoryId.Should().Be(category.Id);
        budget.AmountLimit.Should().Be(BudgetData.AmountLimit);
        budget.Period.Should().Be(BudgetData.Period);
        budget.Notes.Should().Be(BudgetData.Notes);
        budget.Status.Should().Be(BudgetStatus.Draft);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());
        var updatedAmountLimit = new Money(100.0m, Currency.Usd);
        var updatedPeriod = DateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)));
        var updatedNotes = "Updated Notes";

        // Act
        var result = budget.Update(
            updatedAmountLimit,
            updatedPeriod,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        budget.AmountLimit.Should().Be(updatedAmountLimit);
        budget.Period.Should().Be(updatedPeriod);
        budget.Notes.Should().Be(updatedNotes);
        budget.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_Should_Fail_WhenIsClosed()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());
        var updatedAmountLimit = new Money(100.0m, Currency.Usd);
        var updatedPeriod = DateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)));
        var updatedNotes = "Updated Notes";

        budget.Close(DateTime.UtcNow);

        // Act
        var result = budget.Update(
            updatedAmountLimit,
            updatedPeriod,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.IsClosed);
    }

    [Fact]
    public void Close_Should_SetProperties()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());

        // Act
        var result = budget.Close(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        budget.Status.Should().Be(BudgetStatus.Closed);
        budget.ClosedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Close_Should_Fail_WhenIsClosed()
    {
        // Arrange
        var budget = BudgetData.GetBudget(CategoryData.GetCategory());
        budget.Close(DateTime.UtcNow);

        // Act
        var result = budget.Close(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.IsClosed);
    }

    [Fact]
    public void AssignTransaction_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        var operationsCount = budget.Transactions.Count;

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        budget.Status.Should().Be(BudgetStatus.Active);
        budget.Transactions.Should().HaveCount(operationsCount + 1);
    }

    [Fact]
    public void AssignTransaction_Should_BudgetUpdatedEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = budget.AssignTransaction(transaction);
        budget = result.Value;

        // Assert
        var domainEvent = AssertDomainEventWasPublished<BudgetUpdatedEvent>(budget);
        domainEvent.Should().NotBeNull();
        domainEvent!.Budget.Id.Should().Be(budget.Id);
    }

    [Fact]
    public void AssignTransaction_Should_Fail_WhenBudgetIsClosed()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        budget.Close(DateTime.UtcNow);

        // Act
        var result = budget.AssignTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.IsClosed);
    }

    [Fact]
    public void AssignTransaction_Should_Fail_WhenTransactionCategoryMismatch()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);

        var newCategory = CategoryData.GetCategory();
        var transaction = TransactionData.GetTransaction(newCategory, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), budget.Period.Start, DateTime.UtcNow);

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
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1), DateTime.UtcNow);

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
        transaction.UndoPayment();

        var operationsCount = budget.Transactions.Count;

        // Act
        var result = budget.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        budget.Transactions.Should().HaveCount(operationsCount - 1);
    }

    [Fact]
    public void RemoveTransaction_Should_RaiseBudgetUpdatedEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);

        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        budget.AssignTransaction(transaction);
        budget.ClearDomainEvents();
        transaction.UndoPayment();

        // Act
        var result = budget.RemoveTransaction(transaction);
        budget = result.Value;

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<BudgetUpdatedEvent>(budget);
        domainEvent.Should().NotBeNull();
        domainEvent!.Budget.Id.Should().Be(budget.Id);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenBudgetIsClosed()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), budget.Period.Start, DateTime.UtcNow);
        budget.AssignTransaction(transaction);
        budget.Close(DateTime.UtcNow);

        // Act
        var result = budget.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(BudgetErrors.IsClosed);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenTransactionNotBelongs()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var budget = BudgetData.GetBudget(category);
        var transaction = TransactionData.GetTransaction(category, PaymentMethodData.GetPaymentMethod());
        transaction.Pay(WalletData.GetWallet(), budget.Period.Start, DateTime.UtcNow);

        var newCategory = CategoryData.GetCategory();
        var newBudget = BudgetData.GetBudget(newCategory);
        var newTransaction = TransactionData.GetTransaction(newCategory, PaymentMethodData.GetPaymentMethod());
        newTransaction.Pay(WalletData.GetWallet(), newBudget.Period.Start, DateTime.UtcNow);
        budget.AssignTransaction(transaction);

        // Act
        var result = budget.RemoveTransaction(newTransaction);

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