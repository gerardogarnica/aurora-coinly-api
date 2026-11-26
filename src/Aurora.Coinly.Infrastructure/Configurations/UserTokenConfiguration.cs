using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens");

        builder.HasKey(x => x.UserTokenId);

        builder.HasXminConcurrencyToken();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AccessTokenId)
            .IsRequired()
            .HasMaxLength(UserToken.MaxAccessTokenIdLength);

        builder.Property(x => x.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(UserToken.RefreshTokenHashLength)
            .IsFixedLength();

        builder.Property(x => x.AccessTokenExpiresOnUtc)
            .IsRequired();

        builder.Property(x => x.RefreshTokenExpiresOnUtc)
            .IsRequired();

        builder.Property(x => x.IssuedOnUtc)
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .IsRequired();

        builder.HasOne<User>(x => x.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.RefreshTokenHash)
            .IsUnique();

        builder.HasIndex(x => x.RefreshTokenExpiresOnUtc);
    }
}
