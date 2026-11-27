namespace Aurora.Coinly.Domain.UnitTests.Users;

public class EmailTests
{
    [Fact]
    public void Create_Should_Succeed_WhenValueIsValid()
    {
        // Arrange

        // Act
        Result<Email> result = Email.Create("user@example.com");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_Should_TrimAndLowercaseValue()
    {
        // Arrange

        // Act
        Result<Email> result = Email.Create("  User@Example.COM  ");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_Should_Succeed_WhenValueHasMaxLength()
    {
        // Arrange
        const string domain = "@example.com";
        string value = new string('a', Email.MaxLength - domain.Length) + domain;

        // Act
        Result<Email> result = Email.Create(value);

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
        Result<Email> result = Email.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(EmailErrors.Empty);
    }

    [Fact]
    public void Create_Should_Fail_WhenValueIsTooLong()
    {
        // Arrange
        const string domain = "@example.com";
        string value = new string('a', Email.MaxLength - domain.Length + 1) + domain;

        // Act
        Result<Email> result = Email.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(EmailErrors.TooLong);
    }

    [Theory]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("Name <user@example.com>")]
    [InlineData("user@example")]
    [InlineData("user@example.")]
    public void Create_Should_Fail_WhenFormatIsInvalid(string value)
    {
        // Arrange

        // Act
        Result<Email> result = Email.Create(value);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(EmailErrors.InvalidFormat);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnValue()
    {
        // Arrange
        Email email = Email.Create("user@example.com").Value;

        // Act
        string value = email;

        // Assert
        value.Should().Be("user@example.com");
    }
}
