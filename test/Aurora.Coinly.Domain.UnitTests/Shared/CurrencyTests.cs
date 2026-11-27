namespace Aurora.Coinly.Domain.UnitTests.Shared;

public class CurrencyTests
{
    [Fact]
    public void FromCode_Should_ReturnKnownCurrency()
    {
        // Arrange

        // Act
        var currency = Currency.FromCode("USD");

        // Assert
        currency.Should().Be(Currency.Usd);
    }
}
