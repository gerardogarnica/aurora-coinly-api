using Aurora.Coinly.Domain.Users.Events;

namespace Aurora.Coinly.Domain.Users;

public sealed class User : BaseEntity
{
    public const int MaxNameLength = 100;
    private const string UnknownInitials = "U";

    private readonly List<UserToken> _tokens = [];
    private readonly List<Role> _roles = [];

    public Email Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => FormatFullName(FirstName, LastName);
    public string Initials => FormatInitials(FirstName, LastName);
    private Password Password { get; set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }

    public IReadOnlyCollection<UserToken> Tokens => _tokens.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public bool IsAdministrator => _roles.Any(r => r.Name == Role.Administrator.Name);

    private User() : base(Guid.Empty) { } // EF Core

    // Read projections select FirstName/LastName instead of the User entity (whose owned Roles
    // collection EF always joins), then format them with the same rules as FullName/Initials.
    public static string FormatFullName(string firstName, string lastName) => $"{firstName} {lastName}";

    public static string FormatInitials(string? firstName, string? lastName) =>
        string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)
            ? UnknownInitials
            : $"{char.ToUpperInvariant(firstName[0])}{char.ToUpperInvariant(lastName[0])}";

    private User(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        Password password,
        DateTime createdOnUtc) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        IsActive = true;
        CreatedOnUtc = createdOnUtc;
    }

    public static Result<User> Create(
        string firstName,
        string lastName,
        Email email,
        Password password,
        DateTime createdOnUtc)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Fail<User>(UserErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Fail<User>(UserErrors.LastNameRequired);
        }

        var user = new User(
            Guid.NewGuid(),
            firstName.Trim(),
            lastName.Trim(),
            email,
            password,
            createdOnUtc);

        // Roles are owned by each user, so the user gets its own instance, not the shared static one.
        user._roles.Add(Role.Member.Copy());

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));

        return user;
    }

    public Result Update(string firstName, string lastName, DateTime updatedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Fail(UserErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Fail(UserErrors.LastNameRequired);
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedOnUtc = updatedOnUtc;

        return Result.Ok();
    }

    public bool VerifyPassword(IPasswordHasher passwordHasher, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        return passwordHasher.VerifyHashedPassword(Password.Hash, providedPassword);
    }

    public Result ChangePassword(Password newPassword, DateTime updatedOnUtc)
    {
        Password = newPassword;
        UpdatedOnUtc = updatedOnUtc;

        AddDomainEvent(new UserPasswordChangedDomainEvent(Id));

        return Result.Ok();
    }

    public Result<UserToken> IssueToken(
        string accessTokenId,
        string refreshTokenHash,
        DateTime accessTokenExpiresOnUtc,
        DateTime refreshTokenExpiresOnUtc,
        DateTime issuedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(accessTokenId))
        {
            return Result.Fail<UserToken>(UserTokenErrors.AccessTokenIdRequired);
        }

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return Result.Fail<UserToken>(UserTokenErrors.RefreshTokenHashRequired);
        }

        if (accessTokenExpiresOnUtc <= issuedOnUtc)
        {
            return Result.Fail<UserToken>(UserTokenErrors.InvalidExpiration);
        }

        if (refreshTokenExpiresOnUtc <= issuedOnUtc)
        {
            return Result.Fail<UserToken>(UserTokenErrors.InvalidExpiration);
        }

        var token = UserToken.Create(
            Id,
            accessTokenId,
            refreshTokenHash,
            accessTokenExpiresOnUtc,
            refreshTokenExpiresOnUtc,
            issuedOnUtc);

        _tokens.Add(token);

        AddDomainEvent(new UserTokenIssuedDomainEvent(Id, token.UserTokenId));

        return token;
    }

    public Result RevokeToken(Guid userTokenId)
    {
        var token = _tokens.FirstOrDefault(t => t.UserTokenId == userTokenId);

        if (token is null)
        {
            return Result.Fail(UserTokenErrors.NotFound);
        }

        Result revokeResult = token.Revoke();

        if (!revokeResult.IsSuccessful)
        {
            return revokeResult;
        }

        AddDomainEvent(new UserTokenRevokedDomainEvent(Id, userTokenId));

        return Result.Ok();
    }

    public Result RevokeAllActiveTokens(DateTime utcNow)
    {
        // Expired tokens can no longer be redeemed, so they need no revocation (nor an event).
        foreach (UserToken token in _tokens.Where(t => t.IsRefreshTokenValid(utcNow)))
        {
            token.Revoke();

            AddDomainEvent(new UserTokenRevokedDomainEvent(Id, token.UserTokenId));
        }

        return Result.Ok();
    }
}