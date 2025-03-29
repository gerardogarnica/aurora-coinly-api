using Aurora.Coinly.Domain.UnitTests.Categories;
using Aurora.Coinly.Domain.UnitTests.Methods;
using Aurora.Coinly.Domain.UnitTests.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Transactions;

public class TransactionTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();

        // Act
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Assert
        transaction.Description.Should().Be(TransactionData.Description);
        transaction.CategoryId.Should().Be(category.Id);
        transaction.TransactionDate.Should().Be(transactionDate);
        transaction.MaxPaymentDate.Should().Be(transactionDate);
        transaction.Amount.Should().Be(TransactionData.Amount);
        transaction.PaymentMethodId.Should().Be(paymentMethod.Id);
        transaction.Notes.Should().Be(TransactionData.Notes);
        transaction.InstallmentNumber.Should().Be(TransactionData.InstallmentNumber);
        transaction.Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public void Create_Should_RaiseTransactionCreatedEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();

        // Act
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<TransactionCreatedEvent>(transaction);
        domainEvent.Should().NotBeNull();
        domainEvent!.Transaction.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public void Create_Should_Fail_WhenCategoryIsDeleted()
    {
        // Arrange
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var category = CategoryData.GetCategory();
        category.Delete(DateTime.UtcNow);
        var paymentMethod = PaymentMethodData.GetPaymentMethod();

        // Act
        var result = Transaction.Create(
            TransactionData.Description,
            category,
            transactionDate,
            transactionDate,
            TransactionData.Amount,
            paymentMethod,
            TransactionData.Notes,
            TransactionData.InstallmentNumber,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(CategoryErrors.IsDeleted);
    }

    [Fact]
    public void Create_Should_Fail_WhenPaymentMethodIsDeleted()
    {
        // Arrange
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        paymentMethod.Delete(DateTime.UtcNow);

        // Act
        var result = Transaction.Create(
            TransactionData.Description,
            category,
            transactionDate,
            transactionDate,
            TransactionData.Amount,
            paymentMethod,
            TransactionData.Notes,
            TransactionData.InstallmentNumber,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PaymentMethodErrors.IsDeleted);
    }

    [Fact]
    public void Pay_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var wallet = WalletData.GetWallet();

        // Act
        var result = transaction.Pay(
            wallet,
            paymentDate,
            DateTime.UtcNow);

        transaction = result.Value;

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Paid);
        transaction.PaymentDate.Should().Be(paymentDate);
        transaction.WalletId.Should().NotBeNull();
        transaction.WalletId!.Should().Be(wallet.Id);
        transaction.PaidOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Pay_Should_RaiseTransactionPaidEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var wallet = WalletData.GetWallet();

        // Act
        var result = transaction.Pay(
            wallet,
            paymentDate,
            DateTime.UtcNow);

        transaction = result.Value;

        // Assert
        var domainEvent = AssertDomainEventWasPublished<TransactionPaidEvent>(transaction);
        domainEvent.Should().NotBeNull();
        domainEvent!.Transaction.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public void Pay_Should_Fail_WhenTransactionIsNotPending()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var wallet = WalletData.GetWallet();

        // Act
        transaction.Remove(DateTime.UtcNow);

        var result = transaction.Pay(
            wallet,
            paymentDate,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.NotPending);
    }

    [Fact]
    public void Remove_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Act
        var result = transaction.Remove(DateTime.UtcNow);
        transaction = result.Value;

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Removed);
        transaction.RemovedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Remove_Should_RaiseTransactionRemovedEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Act
        var result = transaction.Remove(DateTime.UtcNow);
        transaction = result.Value;

        // Assert
        var domainEvent = AssertDomainEventWasPublished<TransactionRemovedEvent>(transaction);
        domainEvent.Should().NotBeNull();
        domainEvent!.Transaction.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public void Remove_Should_Fail_WhenTransactionIsPaid()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Act
        transaction.Pay(
            WalletData.GetWallet(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        var result = transaction.Remove(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.AlreadyPaid);
    }

    [Fact]
    public void Remove_Should_Fail_WhenTransactionIsRemoved()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Act
        transaction.Remove(DateTime.UtcNow);
        var result = transaction.Remove(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.AlreadyRemoved);
    }

    [Fact]
    public void UndoPayment_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        transaction.Pay(
            WalletData.GetWallet(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Act
        var result = transaction.UndoPayment();
        transaction = result.Value;

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.PaymentDate.Should().BeNull();
        transaction.WalletId.Should().BeNull();
        transaction.PaidOnUtc.Should().BeNull();
    }

    [Fact]
    public void UndoPayment_Should_RaiseTransactionPaymentUndoneEvent()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        transaction.Pay(
            WalletData.GetWallet(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Act
        var result = transaction.UndoPayment();
        transaction = result.Value;

        // Assert
        var domainEvent = AssertDomainEventWasPublished<TransactionPaymentUndoneEvent>(transaction);
        domainEvent.Should().NotBeNull();
        domainEvent!.Transaction.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public void UndoPayment_Should_Fail_WhenTransactionIsNotPaid()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var transaction = TransactionData.GetTransaction(category, paymentMethod);

        // Act
        var result = transaction.UndoPayment();

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.NotPaid);
    }
}