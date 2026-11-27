using Aurora.Coinly.Domain.Users.Events;

namespace Aurora.Coinly.Domain.UnitTests.Users;

public class UserTests : BaseTest
{
    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";

        public bool VerifyHashedPassword(string hashedPassword, string providedPassword) =>
            hashedPassword == HashPassword(providedPassword);

        public void VerifyDummy(string providedPassword)
        {
            // Not used by the domain; only part of the interface.
        }
    }

    private static readonly FakePasswordHasher PasswordHasher = new();

    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange

        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.Password,
            UserData.CreatedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Email.Should().Be(UserData.Email);
        result.Value.FirstName.Should().Be(UserData.FirstName);
        result.Value.LastName.Should().Be(UserData.LastName);
        result.Value.IsActive.Should().BeTrue();
        result.Value.CreatedOnUtc.Should().Be(UserData.CreatedOnUtc);
        result.Value.UpdatedOnUtc.Should().BeNull();
        result.Value.Tokens.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_TrimNames()
    {
        // Arrange

        // Act
        Result<User> result = User.Create(
            "  First  ",
            "  Last  ",
            UserData.Email,
            UserData.Password,
            UserData.CreatedOnUtc);

        // Assert
        result.Value.FirstName.Should().Be("First");
        result.Value.LastName.Should().Be("Last");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenFirstNameIsEmpty(string firstName)
    {
        // Arrange

        // Act
        Result<User> result = User.Create(
            firstName,
            UserData.LastName,
            UserData.Email,
            UserData.Password,
            UserData.CreatedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserErrors.FirstNameRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenLastNameIsEmpty(string lastName)
    {
        // Arrange

        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            lastName,
            UserData.Email,
            UserData.Password,
            UserData.CreatedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserErrors.LastNameRequired);
    }

    [Fact]
    public void Create_Should_AddMemberRole()
    {
        // Arrange

        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.Password,
            UserData.CreatedOnUtc);

        // Assert
        result.Value.Roles.Should().ContainSingle(role => role.Name == Role.Member.Name);
        result.Value.IsAdministrator.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_GiveEachUserItsOwnRoleInstance()
    {
        // Arrange

        // Act
        User first = UserData.GetUser();
        User second = UserData.GetUser();

        // Assert
        first.Roles.Single().Should().NotBeSameAs(Role.Member);
        first.Roles.Single().Should().NotBeSameAs(second.Roles.Single());
    }

    [Fact]
    public void Create_Should_RaiseUserCreatedEvent()
    {
        // Arrange

        // Act
        User user = UserData.GetUser();

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserCreatedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        User user = UserData.GetUser();
        DateTime updatedOnUtc = UserData.CreatedOnUtc.AddDays(1);

        // Act
        Result result = user.Update("  UpdatedFirst  ", "  UpdatedLast  ", updatedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        user.FirstName.Should().Be("UpdatedFirst");
        user.LastName.Should().Be("UpdatedLast");
        user.UpdatedOnUtc.Should().Be(updatedOnUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_Should_Fail_WhenFirstNameIsEmpty(string firstName)
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result result = user.Update(firstName, UserData.LastName, UserData.CreatedOnUtc.AddDays(1));

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserErrors.FirstNameRequired);
        user.FirstName.Should().Be(UserData.FirstName);
        user.UpdatedOnUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_Should_Fail_WhenLastNameIsEmpty(string lastName)
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result result = user.Update(UserData.FirstName, lastName, UserData.CreatedOnUtc.AddDays(1));

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserErrors.LastNameRequired);
        user.LastName.Should().Be(UserData.LastName);
        user.UpdatedOnUtc.Should().BeNull();
    }

    [Fact]
    public void FullName_Should_JoinFirstAndLastName()
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        string fullName = user.FullName;

        // Assert
        fullName.Should().Be("First Last");
    }

    [Theory]
    [InlineData("ana", "lopez", "AL")]
    [InlineData("Ana", "López", "AL")]
    [InlineData("", "Lopez", "U")]
    [InlineData("Ana", "", "U")]
    [InlineData(null, "Lopez", "U")]
    [InlineData("Ana", null, "U")]
    public void FormatInitials_Should_ReturnUppercaseInitials(string? firstName, string? lastName, string expected)
    {
        // Arrange

        // Act
        string initials = User.FormatInitials(firstName, lastName);

        // Assert
        initials.Should().Be(expected);
    }

    [Fact]
    public void VerifyPassword_Should_ReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            Password.Create(PasswordHasher.HashPassword("Secret123!")).Value,
            UserData.CreatedOnUtc).Value;

        // Act
        bool isValid = user.VerifyPassword(PasswordHasher, "Secret123!");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            Password.Create(PasswordHasher.HashPassword("Secret123!")).Value,
            UserData.CreatedOnUtc).Value;

        // Act
        bool isValid = user.VerifyPassword(PasswordHasher, "Wrong123!");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_Should_ReplacePassword()
    {
        // Arrange
        User user = UserData.GetUser();
        Password newPassword = Password.Create(PasswordHasher.HashPassword("NewSecret123!")).Value;
        DateTime updatedOnUtc = UserData.CreatedOnUtc.AddDays(1);

        // Act
        Result result = user.ChangePassword(newPassword, updatedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        user.VerifyPassword(PasswordHasher, "NewSecret123!").Should().BeTrue();
        user.UpdatedOnUtc.Should().Be(updatedOnUtc);
    }

    [Fact]
    public void ChangePassword_Should_RejectOldPassword()
    {
        // Arrange
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            Password.Create(PasswordHasher.HashPassword("Secret123!")).Value,
            UserData.CreatedOnUtc).Value;
        Password newPassword = Password.Create(PasswordHasher.HashPassword("NewSecret123!")).Value;

        // Act
        user.ChangePassword(newPassword, UserData.CreatedOnUtc.AddDays(1));

        // Assert
        user.VerifyPassword(PasswordHasher, "Secret123!").Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_Should_RaiseUserPasswordChangedEvent()
    {
        // Arrange
        User user = UserData.GetUser();
        Password newPassword = Password.Create(PasswordHasher.HashPassword("NewSecret123!")).Value;

        // Act
        user.ChangePassword(newPassword, UserData.CreatedOnUtc.AddDays(1));

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserPasswordChangedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);
    }
}
