using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Shared;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetTransactionConfiguration : IEntityTypeConfiguration<BudgetTransaction>
{
    public void Configure(EntityTypeBuilder<BudgetTransaction> builder)
    {
        builder.ToTable("budget_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(100);

        builder.OwnsOne(x => x.Amount, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder
            .HasOne<Budget>()
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.BudgetId)
            .IsRequired();
    }
}