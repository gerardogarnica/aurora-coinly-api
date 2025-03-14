using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Shared;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.AmountLimit, y =>
        {
            y.Property(x => x.Amount).HasColumnType("numeric(9, 2)");
            y.Property(x => x.Currency)
                .HasConversion(x => x.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.Period, y =>
        {
            y.Property(x => x.Start).HasColumnType("date");
            y.Property(x => x.End).HasColumnType("date");
        });

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired();
    }
}