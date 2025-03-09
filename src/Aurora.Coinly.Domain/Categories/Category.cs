﻿using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Categories;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }

    private Category() : base(Guid.NewGuid())
    {
        Name = string.Empty;
    }

    public static Category Create(
        string name,
        TransactionType type,
        string? notes)
    {
        var category = new Category
        {
            Name = name,
            Type = type,
            Notes = notes,
            IsDeleted = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        return category;
    }

    public Result<Category> Update(string name, string? notes)
    {
        if (IsDeleted)
        {
            return Result.Fail<Category>(CategoryErrors.IsDeleted);
        }

        Name = name;
        Notes = notes;
        UpdatedOnUtc = DateTime.UtcNow;

        return this;
    }

    public Result<Category> Delete()
    {
        if (IsDeleted)
        {
            return Result.Fail<Category>(CategoryErrors.IsDeleted);
        }

        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;

        return this;
    }
}