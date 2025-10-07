using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Users;

public class UserTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange

        // Act
        var user = User.Create(
            UserData.Email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow);

        // Assert
        user.Email.Should().Be(UserData.Email);
        user.FirstName.Should().Be(UserData.FirstName);
        user.LastName.Should().Be(UserData.LastName);
    }

    [Fact]
    public void Create_Should_AddMemberRole()
    {
        // Arrange

        // Act
        var user = User.Create(
            UserData.Email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow);

        // Assert
        user.Roles.Should().Contain(role => role.Name == Role.Member.Name);
    }

    [Fact]
    public void Create_Should_RaiseUserCreatedEvent()
    {
        // Arrange

        // Act
        var user = User.Create(
            UserData.Email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserCreatedEvent>(user);
        domainEvent.Should().NotBeNull();
        domainEvent!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        var user = UserData.GetUser();
        var newFirstName = "UpdatedFirst";
        var newLastName = "UpdatedLast";

        // Act
        user.Update(newFirstName, newLastName, DateTime.UtcNow);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.UpdatedOnUtc.Should().NotBeNull();
    }
}