namespace Aurora.Coinly.Domain.UnitTests.Shared;

public class ColorTests
{
    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#ff5733")]
    [InlineData("blue")]
    [InlineData("primary-500")]
    public void Create_Should_KeepValueAsGiven(string value)
    {
        // Arrange

        // Act
        Result<Color> result = Color.Create(value);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Fact]
    public void Create_Should_TrimValue()
    {
        // Arrange

        // Act
        Result<Color> result = Color.Create("  #FF5733  ");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be("#FF5733");
    }

    [Fact]
    public void Create_Should_Succeed_WhenValueHasMaxLength()
    {
        // Arrange
        string value = new('a', Color.MaxLength);

        // Act
        Result<Color> result = Color.Create(value);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenValueIsEmpty(string value)
    {
        // Arrange

        // Act
        Result<Color> result = Color.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(ColorErrors.Required);
    }

    [Fact]
    public void Create_Should_Fail_WhenValueIsTooLong()
    {
        // Arrange
        string value = new('a', Color.MaxLength + 1);

        // Act
        Result<Color> result = Color.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(ColorErrors.TooLong);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnValue()
    {
        // Arrange
        Color color = Color.Create("#FF5733").Value;

        // Act
        string value = color;

        // Assert
        value.Should().Be("#FF5733");
    }
}
