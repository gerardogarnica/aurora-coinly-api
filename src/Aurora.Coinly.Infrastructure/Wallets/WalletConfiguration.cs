using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Infrastructure.Wallets;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.OwnsOne(x => x.AvailableAmount, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.SavingsAmount, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Ignore(x => x.TotalAmount);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);

        builder.Property(x => x.Notes).HasMaxLength(1000);
    }
}