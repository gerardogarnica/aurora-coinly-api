﻿using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Infrastructure.Transactions;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(100);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);

        builder.OwnsOne(x => x.Amount, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);

        builder.Ignore(x => x.IsPaid);
        builder.Ignore(x => x.IsRecurring);

        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired();

        builder
            .HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .IsRequired();
    }
}