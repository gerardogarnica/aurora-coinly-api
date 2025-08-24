using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Infrastructure.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(40);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired();
    }
}