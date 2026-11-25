using Aurora.Coinly.Domain.Wallets;
using Newtonsoft.Json;

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

    [Fact]
    public void Deserialize_Should_RestoreCode_WhenSerializedWithOutboxSettings()
    {
        // Arrange
        string json = JsonConvert.SerializeObject(Currency.Eur, SerializerSettings.Instance);

        // Act
        var currency = JsonConvert.DeserializeObject<Currency>(json, SerializerSettings.Instance);

        // Assert
        currency.Should().Be(Currency.Eur);
    }

    [Fact]
    public void Deserialize_Should_RestoreMoney_WhenEventCarriesIt()
    {
        // Arrange
        var amount = new Money(150.25m, Currency.Usd);
        IDomainEvent domainEvent = new WalletSavingsUpdatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            new DateOnly(2026, 1, 15),
            true);

        string json = JsonConvert.SerializeObject(domainEvent, SerializerSettings.Instance);

        // Act
        var deserialized = JsonConvert.DeserializeObject<IDomainEvent>(json, SerializerSettings.Instance);

        // Assert
        deserialized.Should().BeOfType<WalletSavingsUpdatedEvent>()
            .Which.Amount.Should().Be(amount);
    }
}
