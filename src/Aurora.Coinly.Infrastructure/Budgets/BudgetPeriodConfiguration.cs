using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Shared;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetPeriodConfiguration : IEntityTypeConfiguration<BudgetPeriod>
{
    public void Configure(EntityTypeBuilder<BudgetPeriod> builder)
    {
        builder.ToTable("budget_periods");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Period, y =>
        {
            y.Property(x => x.Start).HasColumnType("date");
            y.Property(x => x.End).HasColumnType("date");
        });

        builder.OwnsOne(x => x.Limit, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder
            .HasOne<Budget>()
            .WithMany(x => x.Periods)
            .HasForeignKey(x => x.BudgetId)
            .IsRequired();
    }
}