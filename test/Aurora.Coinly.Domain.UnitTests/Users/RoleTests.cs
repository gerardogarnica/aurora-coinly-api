namespace Aurora.Coinly.Domain.UnitTests.Users;

public class RoleTests
{
    [Fact]
    public void All_Should_ContainEveryRole()
    {
        // Arrange

        // Act
        IEnumerable<string> names = Role.All.Select(r => r.Name);

        // Assert
        names.Should().BeEquivalentTo(Role.Administrator.Name, Role.Member.Name);
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("Member")]
    public void FromName_Should_ReturnRole_WhenNameExists(string name)
    {
        // Arrange

        // Act
        Result<Role> result = Role.FromName(name);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public void FromName_Should_ReturnNewInstance()
    {
        // Arrange

        // Act
        Result<Role> result = Role.FromName(Role.Member.Name);

        // Assert
        result.Value.Should().NotBeSameAs(Role.Member);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("member")]
    [InlineData("")]
    public void FromName_Should_Fail_WhenNameDoesNotExist(string name)
    {
        // Arrange

        // Act
        Result<Role> result = Role.FromName(name);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(RoleErrors.NotFound);
    }
}
