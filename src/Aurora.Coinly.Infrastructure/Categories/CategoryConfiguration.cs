﻿using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Infrastructure.Categories;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);

        builder.Property(x => x.Notes).HasMaxLength(1000);
    }
}