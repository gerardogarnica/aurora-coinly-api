using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(100);

        builder.OwnsOne(x => x.Amount, y =>
        {
            y.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric(9, 2)");

            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);

        builder.Ignore(x => x.Type);
        builder.Ignore(x => x.IsPaid);
        builder.Ignore(x => x.IsRecurring);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired();

        builder
            .HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .IsRequired(false);

        builder
            .HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .IsRequired(false);
    }
}