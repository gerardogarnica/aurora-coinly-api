using Aurora.Coinly.Application.Users;
using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IDateTimeService dateTimeService) : ICommandHandler<RegisterUserCommand, UserModel>
{
    public async Task<Result<UserModel>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is not null)
        {
            return Result.Fail<UserModel>(UserErrors.EmailAlreadyExists);
        }

        // Encrypt password
        Password password = Password.Create(request.Password.Trim());
        string hashedPassword = passwordHasher.HashPassword(password.Value);

        // Create user
        var newUser = User.Create(
            request.Email.Trim(),
            request.FirstName.Trim(),
            request.LastName.Trim(),
            hashedPassword,
            Guid.NewGuid().ToString(),
            dateTimeService.UtcNow);

        await userRepository.AddAsync(newUser, cancellationToken);

        return newUser.ToModel();
    }
}