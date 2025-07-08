using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Summary;

namespace Aurora.Coinly.Infrastructure.Summary;

internal sealed class MonthlySummaryConfiguration : IEntityTypeConfiguration<MonthlySummary>
{
    public void Configure(EntityTypeBuilder<MonthlySummary> builder)
    {
        builder.ToTable("monthly_summaries");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Currency, y =>
        {
            y.Property(x => x.Code).HasMaxLength(3);
        });

        builder.Property(x => x.TotalIncome).HasColumnType("numeric(9, 2)");
        builder.Property(x => x.TotalExpense).HasColumnType("numeric(9, 2)");
        builder.Property(x => x.Savings).HasColumnType("numeric(9, 2)");

        builder.Ignore(x => x.Balance);
    }
}