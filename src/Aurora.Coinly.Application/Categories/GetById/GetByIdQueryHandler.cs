﻿using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.GetById;

internal sealed class GetByIdQueryHandler(
    ICategoryRepository categoryRepository) : IQueryHandler<GetByIdQuery, CategoryModel>
{
    public async Task<Result<CategoryModel>> Handle(
        GetByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.Id);
        if (category is null)
        {
            return Result.Fail<CategoryModel>(CategoryErrors.NotFound);
        }

        // Return category model
        return category.ToModel();
    }
}