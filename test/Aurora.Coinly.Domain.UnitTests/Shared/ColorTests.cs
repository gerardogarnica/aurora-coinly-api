namespace Aurora.Coinly.Domain.UnitTests.Shared;

public class ColorTests
{
    [Fact]
    public void FromHex_Should_ReturnUpperCasedColor()
    {
        // Arrange

        // Act
        Result<Color> result = Color.FromHex("#ff5733");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be("#FF5733");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromHex_Should_Fail_WhenValueIsEmpty(string hexValue)
    {
        // Arrange

        // Act
        Result<Color> result = Color.FromHex(hexValue);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(ColorErrors.Empty);
    }

    [Theory]
    [InlineData("FF5733")]
    [InlineData("#FF573")]
    [InlineData("#GG5733")]
    [InlineData("#FF57331")]
    public void FromHex_Should_Fail_WhenFormatIsInvalid(string hexValue)
    {
        // Arrange

        // Act
        Result<Color> result = Color.FromHex(hexValue);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(ColorErrors.InvalidFormat);
    }
}
