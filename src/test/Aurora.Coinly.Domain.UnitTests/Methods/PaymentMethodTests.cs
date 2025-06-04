using Aurora.Coinly.Domain.UnitTests.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Methods;

public class PaymentMethodTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange
        var wallet = WalletData.GetWallet();

        // Act
        var paymentMethod = PaymentMethod.Create(
            PaymentMethodData.Name,
            PaymentMethodData.IsDefault,
            PaymentMethodData.AllowRecurring,
            PaymentMethodData.AutoMarkAsPaid,
            wallet,
            PaymentMethodData.MaxDaysToReverse,
            PaymentMethodData.SuggestedPaymentDay,
            PaymentMethodData.StatementCutoffDay,
            PaymentMethodData.Notes,
            DateTime.UtcNow);

        // Assert
        paymentMethod.Name.Should().Be(PaymentMethodData.Name);
        paymentMethod.IsDefault.Should().Be(PaymentMethodData.IsDefault);
        paymentMethod.AllowRecurring.Should().Be(PaymentMethodData.AllowRecurring);
        paymentMethod.AutoMarkAsPaid.Should().Be(PaymentMethodData.AutoMarkAsPaid);
        paymentMethod.WalletId.Should().Be(wallet.Id);
        paymentMethod.SuggestedPaymentDay.Should().Be(PaymentMethodData.SuggestedPaymentDay);
        paymentMethod.Notes.Should().Be(PaymentMethodData.Notes);
        paymentMethod.IsDeleted.Should().Be(false);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var updatedName = "Updated Name";
        var updatedWallet = WalletData.GetWallet();
        var updatedMaxDaysToReverse = 15;
        var updatedSuggestedPaymentDay = 20;
        var updatedStatementCutoffDay = 25;
        var updatedNotes = "Updated Notes";

        // Act
        var result = paymentMethod.Update(
            updatedName,
            PaymentMethodData.AllowRecurring,
            PaymentMethodData.AutoMarkAsPaid,
            updatedWallet,
            updatedMaxDaysToReverse,
            updatedSuggestedPaymentDay,
            updatedStatementCutoffDay,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        paymentMethod.Name.Should().Be(updatedName);
        paymentMethod.AllowRecurring.Should().Be(PaymentMethodData.AllowRecurring);
        paymentMethod.AutoMarkAsPaid.Should().Be(PaymentMethodData.AutoMarkAsPaid);
        paymentMethod.WalletId.Should().Be(updatedWallet.Id);
        paymentMethod.SuggestedPaymentDay.Should().Be(updatedSuggestedPaymentDay);
        paymentMethod.StatementCutoffDay.Should().Be(updatedStatementCutoffDay);
        paymentMethod.Notes.Should().Be(updatedNotes);
        paymentMethod.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        var updatedName = "Updated Name";
        var updatedWallet = WalletData.GetWallet();
        var updatedMaxDaysToReverse = 15;
        var updatedSuggestedPaymentDay = 20;
        var updatedStatementCutoffDay = 25;
        var updatedNotes = "Updated Notes";

        paymentMethod.Delete(DateTime.UtcNow);

        // Act
        var result = paymentMethod.Update(
            updatedName,
            PaymentMethodData.AllowRecurring,
            PaymentMethodData.AutoMarkAsPaid,
            updatedWallet,
            updatedMaxDaysToReverse,
            updatedSuggestedPaymentDay,
            updatedStatementCutoffDay,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PaymentMethodErrors.IsDeleted);
    }

    [Fact]
    public void Delete_Should_SetProperties()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();

        // Act
        var result = paymentMethod.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        paymentMethod.IsDeleted.Should().BeTrue();
        paymentMethod.DeletedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Delete_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        paymentMethod.Delete(DateTime.UtcNow);

        // Act
        var result = paymentMethod.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PaymentMethodErrors.IsDeleted);
    }

    [Fact]
    public void SetAsDefault_Should_SetProperties()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();

        // Act
        var result = paymentMethod.SetAsDefault(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        paymentMethod.IsDefault.Should().BeTrue();
        paymentMethod.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void SetAsDefault_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        paymentMethod.Delete(DateTime.UtcNow);

        // Act
        var result = paymentMethod.SetAsDefault(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PaymentMethodErrors.IsDeleted);
    }

    [Fact]
    public void SetAsNotDefault_Should_SetProperties()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        paymentMethod.SetAsDefault(DateTime.UtcNow);

        // Act
        var result = paymentMethod.SetAsNotDefault(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        paymentMethod.IsDefault.Should().BeFalse();
        paymentMethod.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void SetAsNotDefault_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var paymentMethod = PaymentMethodData.GetPaymentMethod();
        paymentMethod.Delete(DateTime.UtcNow);
        paymentMethod.SetAsDefault(DateTime.UtcNow);

        // Act
        var result = paymentMethod.SetAsNotDefault(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PaymentMethodErrors.IsDeleted);
    }
}