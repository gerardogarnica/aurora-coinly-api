using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Users;

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

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.IdentityId).IsUnique();
    }
}