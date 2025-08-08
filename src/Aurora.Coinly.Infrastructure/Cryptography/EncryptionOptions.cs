namespace Aurora.Coinly.Infrastructure.Cryptography;

public sealed class EncryptionOptions
{
    public const string SectionName = "Encryption";

    public string MasterKey { get; init; }
}