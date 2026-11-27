namespace Aurora.Coinly.Infrastructure.Database;

internal static class ConcurrencyTokenExtensions
{
    private const string VersionPropertyName = "Version";
    private const string VersionColumnName = "xmin";

    internal static EntityTypeBuilder<TEntity> HasXminConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        // Explicit column name so EFCore.NamingConventions does not rename it to "version".
        builder.Property<uint>(VersionPropertyName)
            .IsRowVersion()
            .HasColumnName(VersionColumnName);

        return builder;
    }
}
