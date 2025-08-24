using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Name);

        builder.Property(x => x.Name).HasMaxLength(50);

        builder.HasData(
            Role.Member,
            Role.Administrator);

        builder
            .HasMany<User>()
            .WithMany(x => x.Roles)
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable("user_roles");

                joinBuilder.Property("RolesName").HasColumnName("role_name");
            });
    }
}