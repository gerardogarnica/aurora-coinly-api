namespace Aurora.Coinly.Domain.Users;

public sealed class User : BaseEntity
{
    private string _passwordHash;
    private readonly List<Role> _roles = [];

    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public string IdentityId { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private User() : base(Guid.NewGuid())
    {
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        IdentityId = string.Empty;
    }

    public static User Create(
        string email,
        string firstName,
        string lastName,
        string password,
        string identityId,
        DateTime createdOnUtc)
    {
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            _passwordHash = password,
            IdentityId = identityId,
            CreatedOnUtc = createdOnUtc
        };

        user._roles.Add(Role.Member);

        user.AddDomainEvent(new UserCreatedEvent(user.Id));

        return user;
    }

    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    public bool VerifyPassword(Password password, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(password.Value))
        {
            return false;
        }

        return passwordHasher.VerifyHashedPassword(_passwordHash, password.Value);
    }
}