using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.IdentityId).HasMaxLength(256);

        builder.Property<string>("_passwordHash")
            .HasField("_passwordHash")
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(256);

        builder.Ignore(user => user.FullName);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.IdentityId).IsUnique();
    }
}