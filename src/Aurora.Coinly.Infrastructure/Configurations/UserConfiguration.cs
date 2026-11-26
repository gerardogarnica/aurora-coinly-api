using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Email, y =>
        {
            y.Property(e => e.Value)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(Email.MaxLength);

            y.HasIndex(e => e.Value).IsUnique();
        });

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.IdentityId).HasMaxLength(256);

        builder.Property<string>("_passwordHash")
            .HasField("_passwordHash")
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(256);

        builder.Ignore(user => user.FullName);

        builder.HasIndex(x => x.IdentityId).IsUnique();
    }
}