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

        builder.OwnsOne(x => x.TotalIncome, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.TotalExpense, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Ignore(x => x.Balance);
    }
}