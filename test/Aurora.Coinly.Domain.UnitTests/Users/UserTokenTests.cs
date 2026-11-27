using Aurora.Coinly.Domain.Users.Events;

namespace Aurora.Coinly.Domain.UnitTests.Users;

public class UserTokenTests : BaseTest
{
    [Fact]
    public void IssueToken_Should_AddTokenToUser()
    {
        // Arrange
        User user = UserData.GetUser();
        string refreshTokenHash = UserData.NewRefreshTokenHash();

        // Act
        Result<UserToken> result = user.IssueToken(
            UserData.AccessTokenId,
            refreshTokenHash,
            UserData.AccessTokenExpiresOnUtc,
            UserData.RefreshTokenExpiresOnUtc,
            UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        UserToken token = result.Value;
        user.Tokens.Should().ContainSingle().Which.Should().BeSameAs(token);
        token.UserTokenId.Should().NotBeEmpty();
        token.UserId.Should().Be(user.Id);
        token.AccessTokenId.Should().Be(UserData.AccessTokenId);
        token.RefreshTokenHash.Should().Be(refreshTokenHash);
        token.AccessTokenExpiresOnUtc.Should().Be(UserData.AccessTokenExpiresOnUtc);
        token.RefreshTokenExpiresOnUtc.Should().Be(UserData.RefreshTokenExpiresOnUtc);
        token.IssuedOnUtc.Should().Be(UserData.IssuedOnUtc);
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IssueToken_Should_RaiseUserTokenIssuedEvent()
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        UserToken token = UserData.IssueToken(user);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserTokenIssuedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.UserTokenId.Should().Be(token.UserTokenId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IssueToken_Should_Fail_WhenAccessTokenIdIsEmpty(string accessTokenId)
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result<UserToken> result = user.IssueToken(
            accessTokenId,
            UserData.NewRefreshTokenHash(),
            UserData.AccessTokenExpiresOnUtc,
            UserData.RefreshTokenExpiresOnUtc,
            UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.AccessTokenIdRequired);
        user.Tokens.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IssueToken_Should_Fail_WhenRefreshTokenHashIsEmpty(string refreshTokenHash)
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result<UserToken> result = user.IssueToken(
            UserData.AccessTokenId,
            refreshTokenHash,
            UserData.AccessTokenExpiresOnUtc,
            UserData.RefreshTokenExpiresOnUtc,
            UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.RefreshTokenHashRequired);
        user.Tokens.Should().BeEmpty();
    }

    [Fact]
    public void IssueToken_Should_Fail_WhenAccessTokenDoesNotExpireAfterIssue()
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result<UserToken> result = user.IssueToken(
            UserData.AccessTokenId,
            UserData.NewRefreshTokenHash(),
            UserData.IssuedOnUtc,
            UserData.RefreshTokenExpiresOnUtc,
            UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.InvalidExpiration);
        user.Tokens.Should().BeEmpty();
    }

    [Fact]
    public void IssueToken_Should_Fail_WhenRefreshTokenDoesNotExpireAfterIssue()
    {
        // Arrange
        User user = UserData.GetUser();

        // Act
        Result<UserToken> result = user.IssueToken(
            UserData.AccessTokenId,
            UserData.NewRefreshTokenHash(),
            UserData.AccessTokenExpiresOnUtc,
            UserData.IssuedOnUtc.AddMinutes(-1),
            UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.InvalidExpiration);
        user.Tokens.Should().BeEmpty();
    }

    [Fact]
    public void RevokeToken_Should_RevokeToken()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);

        // Act
        Result result = user.RevokeToken(token.UserTokenId);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void RevokeToken_Should_KeepOtherTokensValid()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken revoked = UserData.IssueToken(user);
        UserToken other = UserData.IssueToken(user);

        // Act
        user.RevokeToken(revoked.UserTokenId);

        // Assert
        other.IsRevoked.Should().BeFalse();
        other.IsRefreshTokenValid(UserData.IssuedOnUtc).Should().BeTrue();
    }

    [Fact]
    public void RevokeToken_Should_RaiseUserTokenRevokedEvent()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);

        // Act
        user.RevokeToken(token.UserTokenId);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserTokenRevokedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.UserTokenId.Should().Be(token.UserTokenId);
    }

    [Fact]
    public void RevokeToken_Should_Fail_WhenTokenIsNotFound()
    {
        // Arrange
        User user = UserData.GetUser();
        UserData.IssueToken(user);

        // Act
        Result result = user.RevokeToken(Guid.NewGuid());

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.NotFound);
    }

    [Fact]
    public void RevokeToken_Should_Fail_WhenTokenIsAlreadyRevoked()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);
        user.RevokeToken(token.UserTokenId);

        // Act
        Result result = user.RevokeToken(token.UserTokenId);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(UserTokenErrors.AlreadyRevoked);
        user.DomainEvents.OfType<UserTokenRevokedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RevokeAllActiveTokens_Should_RevokeOnlyValidTokens()
    {
        // Arrange
        User user = UserData.GetUser();
        DateTime utcNow = UserData.IssuedOnUtc.AddHours(2);

        UserToken valid = UserData.IssueToken(user);
        UserToken expired = UserData.IssueToken(user, UserData.IssuedOnUtc.AddHours(1));
        UserToken revoked = UserData.IssueToken(user);
        user.RevokeToken(revoked.UserTokenId);
        user.ClearDomainEvents();

        // Act
        Result result = user.RevokeAllActiveTokens(utcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        valid.IsRevoked.Should().BeTrue();
        expired.IsRevoked.Should().BeFalse();
        revoked.IsRevoked.Should().BeTrue();
        user.DomainEvents.OfType<UserTokenRevokedDomainEvent>()
            .Should().ContainSingle()
            .Which.UserTokenId.Should().Be(valid.UserTokenId);
    }

    [Fact]
    public void RevokeAllActiveTokens_Should_Succeed_WhenUserHasNoTokens()
    {
        // Arrange
        User user = UserData.GetUser();
        user.ClearDomainEvents();

        // Act
        Result result = user.RevokeAllActiveTokens(UserData.IssuedOnUtc);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RevokeAllActiveTokens_Should_RaiseOneEventPerRevokedToken()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken first = UserData.IssueToken(user);
        UserToken second = UserData.IssueToken(user);
        user.ClearDomainEvents();

        // Act
        user.RevokeAllActiveTokens(UserData.IssuedOnUtc.AddHours(1));

        // Assert
        user.DomainEvents.OfType<UserTokenRevokedDomainEvent>()
            .Select(e => e.UserTokenId)
            .Should().BeEquivalentTo([first.UserTokenId, second.UserTokenId]);
    }

    [Fact]
    public void IsRefreshTokenValid_Should_ReturnTrue_WhenNotRevokedAndNotExpired()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);

        // Act
        bool isValid = token.IsRefreshTokenValid(UserData.RefreshTokenExpiresOnUtc.AddSeconds(-1));

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsRefreshTokenValid_Should_ReturnFalse_WhenExpired()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);

        // Act
        bool isValid = token.IsRefreshTokenValid(UserData.RefreshTokenExpiresOnUtc);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsRefreshTokenValid_Should_ReturnFalse_WhenRevoked()
    {
        // Arrange
        User user = UserData.GetUser();
        UserToken token = UserData.IssueToken(user);
        user.RevokeToken(token.UserTokenId);

        // Act
        bool isValid = token.IsRefreshTokenValid(UserData.IssuedOnUtc);

        // Assert
        isValid.Should().BeFalse();
    }
}
