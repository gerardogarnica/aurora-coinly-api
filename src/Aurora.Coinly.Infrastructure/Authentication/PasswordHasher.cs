namespace Aurora.Coinly.Infrastructure.Authentication;

internal sealed class PasswordHasher(EncryptionService encryptionService) : IPasswordHasher
{
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        return encryptionService.Encrypt(password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);
        ArgumentNullException.ThrowIfNull(providedPassword);

        var decryptedPassword = encryptionService.Decrypt(hashedPassword);

        return decryptedPassword == providedPassword;
    }
}