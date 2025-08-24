using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccessToken).HasMaxLength(1000);
        builder.Property(x => x.RefreshToken).HasMaxLength(1000);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired();
    }
}