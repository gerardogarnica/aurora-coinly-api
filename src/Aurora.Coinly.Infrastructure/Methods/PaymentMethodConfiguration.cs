using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Infrastructure.Methods;

internal sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder
            .HasOne(x => x.Wallet)
            .WithMany(x => x.Methods)
            .HasForeignKey(x => x.WalletId)
            .IsRequired();
    }
}