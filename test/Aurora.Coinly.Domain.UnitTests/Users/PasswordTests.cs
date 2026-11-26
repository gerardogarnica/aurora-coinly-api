namespace Aurora.Coinly.Domain.UnitTests.Users;

public class PasswordTests
{
    [Fact]
    public void Create_Should_SetHash()
    {
        // Arrange
        const string hash = "hashed-password";

        // Act
        Result<Password> result = Password.Create(hash);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenHashIsEmpty(string hash)
    {
        // Arrange

        // Act
        Result<Password> result = Password.Create(hash);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PasswordErrors.Required);
    }

    [Fact]
    public void Create_Should_Fail_WhenHashIsTooShort()
    {
        // Arrange

        // Act
        Result<Password> result = Password.Create("Short12");

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PasswordErrors.TooShort);
    }

    [Theory]
    [InlineData(Password.MinHashLength)]
    [InlineData(Password.MaxHashLength)]
    public void Create_Should_Succeed_WhenHashLengthIsAtTheLimit(int length)
    {
        // Arrange
        string hash = new('a', length);

        // Act
        Result<Password> result = Password.Create(hash);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_Fail_WhenHashIsTooLong()
    {
        // Arrange
        string hash = new('a', Password.MaxHashLength + 1);

        // Act
        Result<Password> result = Password.Create(hash);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(PasswordErrors.TooLong);
    }
}
