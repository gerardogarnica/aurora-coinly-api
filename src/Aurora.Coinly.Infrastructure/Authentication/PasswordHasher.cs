using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Aurora.Coinly.Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int IterationsSize = sizeof(int);
    private const int Iterations = 600_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const byte FormatVersion = 0x02;
    private const int IterationsOffset = 1;
    private const int SaltOffset = IterationsOffset + IterationsSize;
    private const int HashOffset = SaltOffset + SaltSize;
    private const int TotalSize = HashOffset + HashSize;

    private static readonly string DummyHash = BuildDummyHash();

    private static string BuildDummyHash()
    {
        byte[] buffer = new byte[TotalSize];
        buffer[0] = FormatVersion;
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(IterationsOffset, IterationsSize), Iterations);
        return Convert.ToBase64String(buffer);
    }

    public void VerifyDummy(string providedPassword) =>
        VerifyHashedPassword(DummyHash, providedPassword);

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        byte[] buffer = new byte[TotalSize];
        buffer[0] = FormatVersion;
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(IterationsOffset, IterationsSize), Iterations);
        Buffer.BlockCopy(salt, 0, buffer, SaltOffset, SaltSize);
        Buffer.BlockCopy(hash, 0, buffer, HashOffset, HashSize);

        return Convert.ToBase64String(buffer);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        byte[] buffer;
        try
        {
            buffer = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return false;
        }

        if (buffer.Length != TotalSize || buffer[0] != FormatVersion)
        {
            return false;
        }

        int iterations = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(IterationsOffset, IterationsSize));
        if (iterations < 1)
        {
            return false;
        }

        byte[] salt = buffer[SaltOffset..HashOffset];
        byte[] storedHash = buffer[HashOffset..];

        byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
