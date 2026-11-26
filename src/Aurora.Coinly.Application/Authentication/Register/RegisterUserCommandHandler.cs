using Aurora.Coinly.Application.Users;

namespace Aurora.Coinly.Application.Authentication.Register;

internal sealed class RegisterUserCommandHandler(
    ICoinlyDbContext dbContext,
    IPasswordHasher passwordHasher,
    IDateTimeService dateTimeService) : ICommandHandler<RegisterUserCommand, UserModel>
{
    public async Task<Result<UserModel>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Get user by email
        Result<Email> email = Email.Create(request.Email);
        if (!email.IsSuccessful)
        {
            return Result.Fail<UserModel>(email.Error);
        }

        string emailValue = email.Value.Value;

        bool emailExists = await dbContext
            .Users
            .AnyAsync(x => x.Email.Value == emailValue, cancellationToken);

        if (emailExists)
        {
            return Result.Fail<UserModel>(UserErrors.EmailAlreadyExists);
        }

        // Encrypt password
        Result<Password> password = Password.Create(request.Password.Trim());
        if (!password.IsSuccessful)
        {
            return Result.Fail<UserModel>(password.Error);
        }

        string hashedPassword = passwordHasher.HashPassword(password.Value.Value);

        // Create user
        var newUser = User.Create(
            email.Value,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            hashedPassword,
            Guid.NewGuid().ToString(),
            dateTimeService.UtcNow);

        dbContext.Users.Add(newUser);

        foreach (Role role in newUser.Roles)
        {
            dbContext.Roles.Attach(role);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return newUser.ToModel();
    }
}