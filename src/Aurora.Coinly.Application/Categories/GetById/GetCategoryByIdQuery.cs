namespace Aurora.Coinly.Application.Categories.GetById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryModel>;