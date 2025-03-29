using Aurora.Coinly.Domain.UnitTests.Categories;
using Aurora.Coinly.Domain.UnitTests.Methods;
using Aurora.Coinly.Domain.UnitTests.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Wallets;

public class WalletTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange

        // Act
        var wallet = Wallet.Create(
            WalletData.Name,
            WalletData.AvailableAmount,
            WalletData.Type,
            WalletData.Notes,
            DateTime.Now);

        // Assert
        wallet.Name.Should().Be(WalletData.Name);
        wallet.AvailableAmount.Should().Be(WalletData.AvailableAmount);
        wallet.SavingsAmount.Should().Be(Money.Zero(WalletData.AvailableAmount.Currency));
        wallet.Type.Should().Be(WalletData.Type);
        wallet.Notes.Should().Be(WalletData.Notes);
        wallet.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_SetCreatedOperation()
    {
        // Arrange

        // Act
        var wallet = Wallet.Create(
            WalletData.Name,
            WalletData.AvailableAmount,
            WalletData.Type,
            WalletData.Notes,
            DateTime.Now);

        // Assert
        wallet.Operations.Should().HaveCount(1);
        wallet.Operations.First().Type.Should().Be(WalletHistoryType.Created);
        wallet.Operations.First().Amount.Should().Be(WalletData.AvailableAmount);
    }

    [Fact]
    public void Create_Should_RaiseWalletCreatedEvent()
    {
        // Arrange

        // Act
        var wallet = Wallet.Create(
            WalletData.Name,
            WalletData.AvailableAmount,
            WalletData.Type,
            WalletData.Notes,
            DateTime.Now);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<WalletCreatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var updatedName = "Updated Name";
        var updatedNotes = "Updated Notes";

        // Act
        var result = wallet.Update(updatedName, updatedNotes, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Name.Should().Be(updatedName);
        wallet.Notes.Should().Be(updatedNotes);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var updatedName = "Updated Name";
        var updatedNotes = "Updated Notes";
        wallet.Delete(DateTime.UtcNow);

        // Act
        var result = wallet.Update(updatedName, updatedNotes, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.IsDeleted);
    }

    [Fact]
    public void AssignToSavings_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var amount = new Money(5.0m, Currency.Usd);
        var availableAmount = wallet.AvailableAmount - amount;
        var savingAmount = wallet.SavingsAmount + amount;

        // Act
        var result = wallet.AssignToSavings(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(availableAmount);
        wallet.SavingsAmount.Should().Be(savingAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void AssignToSavings_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var amount = new Money(5.0m, Currency.Usd);

        // Act
        var result = wallet.AssignToSavings(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void AssignToSavings_Should_SetAssignedToSavingsOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var amount = new Money(5.0m, Currency.Usd);

        // Act
        var result = wallet.AssignToSavings(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.AssignedToSavings);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(amount);
    }

    [Fact]
    public void AssignToSavings_Should_Fail_WhenUnableToAssignToSavings()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var amount = wallet.AvailableAmount + new Money(5.0m, Currency.Usd);

        // Act
        var result = wallet.AssignToSavings(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.UnableToAssignToSavings);
    }

    [Fact]
    public void AssignToAvailable_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        wallet.AssignToSavings(new Money(5.0m, Currency.Usd), DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        var amount = new Money(5.0m, Currency.Usd);
        var availableAmount = wallet.AvailableAmount + amount;
        var savingAmount = wallet.SavingsAmount - amount;

        // Act
        var result = wallet.AssignToAvailable(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(availableAmount);
        wallet.SavingsAmount.Should().Be(savingAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void AssignToAvailable_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        wallet.AssignToSavings(new Money(5.0m, Currency.Usd), DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        var amount = new Money(5.0m, Currency.Usd);
        wallet.ClearDomainEvents();

        // Act
        var result = wallet.AssignToAvailable(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void AssignToAvailable_Should_SetAssignedToAvailableOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        wallet.AssignToSavings(new Money(5.0m, Currency.Usd), DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        var amount = new Money(5.0m, Currency.Usd);

        // Act
        var result = wallet.AssignToAvailable(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.AssignedToAvailable);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(amount);
    }

    [Fact]
    public void AssignToAvailable_Should_Fail_WhenUnableToAssignToAvailable()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var amount = wallet.SavingsAmount + new Money(5.0m, Currency.Usd);

        // Act
        var result = wallet.AssignToAvailable(amount, DateOnly.FromDateTime(DateTime.Now), DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.UnableToAssignToAvailable);
    }

    [Fact]
    public void DepositTransaction_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        var expectedAvailableAmount = wallet.AvailableAmount + transaction.Amount;
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = wallet.Deposit(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(expectedAvailableAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void DepositTransaction_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.ClearDomainEvents();

        // Act
        var result = wallet.Deposit(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void DepositTransaction_Should_AddDepositOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = wallet.Deposit(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.Deposit);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(transaction.Amount);
    }

    [Fact]
    public void DepositTransaction_Should_Fail_WhenTransactionIsNotPaid()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        // Act
        var result = wallet.Deposit(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.NotPaid);
    }

    [Fact]
    public void DepositOperation_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Deposit operation";
        var expectedAvailableAmount = wallet.AvailableAmount + operationAmount;

        // Act
        var result = wallet.Deposit(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(expectedAvailableAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void DepositOperation_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Deposit operation";
        wallet.ClearDomainEvents();

        // Act
        var result = wallet.Deposit(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void DepositOperation_Should_AddDepositOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Deposit operation";

        // Act
        var result = wallet.Deposit(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.Deposit);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(operationAmount);
    }

    [Fact]
    public void WithdrawTransaction_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        var expectedAvailableAmount = wallet.AvailableAmount - transaction.Amount;
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = wallet.Withdraw(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(expectedAvailableAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void WithdrawTransaction_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.ClearDomainEvents();

        // Act
        var result = wallet.Withdraw(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void WithdrawTransaction_Should_AddWithdrawalOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());
        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

        // Act
        var result = wallet.Withdraw(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.Withdrawal);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(transaction.Amount);
    }

    [Fact]
    public void WithdrawTransaction_Should_Fail_WhenTransactionIsNotPaid()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        // Act
        var result = wallet.Withdraw(transaction, DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.NotPaid);
    }

    [Fact]
    public void WithdrawOperation_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Withdraw operation";
        var expectedAvailableAmount = wallet.AvailableAmount - operationAmount;

        // Act
        var result = wallet.Withdraw(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.AvailableAmount.Should().Be(expectedAvailableAmount);
        wallet.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void WithdrawOperation_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Withdraw operation";
        wallet.ClearDomainEvents();

        // Act
        var result = wallet.Withdraw(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void WithdrawOperation_Should_AddWithdrawalOperation()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var operationAmount = new Money(15.0m, Currency.Usd);
        var operationDescription = "Withdraw operation";

        // Act
        var result = wallet.Withdraw(
            operationAmount,
            operationDescription,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.LastOrDefault()!.Type.Should().Be(WalletHistoryType.Withdrawal);
        wallet.Operations.LastOrDefault()!.Amount.Should().Be(operationAmount);
    }

    [Fact]
    public void Delete_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();

        // Act
        var result = wallet.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.IsDeleted.Should().BeTrue();
        wallet.DeletedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Delete_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        wallet.Delete(DateTime.UtcNow);

        // Act
        var result = wallet.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.IsDeleted);
    }

    [Fact]
    public void RemoveTransaction_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.Withdraw(transaction, DateTime.UtcNow);

        transaction.UndoPayment();

        // Act
        var result = wallet.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        wallet.Operations.Any(o => o.TransactionId == transaction.Id).Should().BeFalse();
    }

    [Fact]
    public void RemoveTransaction_Should_RaiseWalletBalanceUpdatedEvent()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.Withdraw(transaction, DateTime.UtcNow);
        wallet.ClearDomainEvents();

        transaction.UndoPayment();

        // Act
        var result = wallet.RemoveTransaction(transaction);
        wallet = result.Value;

        // Assert
        result.IsSuccessful.Should().BeTrue();
        var domainEvent = AssertDomainEventWasPublished<WalletBalanceUpdatedEvent>(wallet);
        domainEvent.Should().NotBeNull();
        domainEvent!.Wallet.Id.Should().Be(wallet.Id);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenWalletIsDeleted()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.Withdraw(transaction, DateTime.UtcNow);

        transaction.UndoPayment();

        wallet.Delete(DateTime.UtcNow);

        // Act
        var result = wallet.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.IsDeleted);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenTransactionNotBelongs()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.Withdraw(transaction, DateTime.UtcNow);

        var newTransaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        // Act
        var result = wallet.RemoveTransaction(newTransaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.TransactionNotBelongs);
    }

    [Fact]
    public void RemoveTransaction_Should_Fail_WhenIsAlreadyPaid()
    {
        // Arrange
        var wallet = WalletData.GetWallet();
        var transaction = TransactionData.GetTransaction(CategoryData.GetCategory(), PaymentMethodData.GetPaymentMethod());

        transaction.Pay(wallet, DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);
        wallet.Withdraw(transaction, DateTime.UtcNow);

        // Act
        var result = wallet.RemoveTransaction(transaction);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(TransactionErrors.AlreadyPaid);
    }
}