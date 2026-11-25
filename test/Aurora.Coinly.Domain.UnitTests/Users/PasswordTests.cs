namespace Aurora.Coinly.Domain.UnitTests.Users;

public class PasswordTests
{
    [Fact]
    public void Create_Should_SetValue()
    {
        // Arrange
        const string value = "Secret123";

        // Act
        Result<Password> result = Password.Create(value);

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
        Result<Password> result = Password.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PasswordErrors.Empty);
    }

    [Fact]
    public void Create_Should_Fail_WhenValueIsTooShort()
    {
        // Arrange

        // Act
        Result<Password> result = Password.Create("Short12");

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PasswordErrors.TooShort);
    }
}
